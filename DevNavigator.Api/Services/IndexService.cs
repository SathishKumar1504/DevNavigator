using System.Security.Cryptography;
using System.Text;
using DevNavigator.Api.Data;
using DevNavigator.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace DevNavigator.Api.Services;

public class IndexService : IIndexService
{
    private static readonly HashSet<string> SupportedExtensions =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ".cs",
            ".cshtml",
            ".js",
            ".jsx",
            ".ts",
            ".tsx",
            ".json",
            ".sql",
            ".scss",
            ".css"
        };

    private static readonly HashSet<string> IgnoredDirectories =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ".git",
            "node_modules",
            "bin",
            "obj",
            "dist",
            "build",
            "coverage"
        };

    private readonly AppDbContext _db;
    private readonly CodeSymbolExtractor _symbolExtractor;

    private async Task IndexSymbolsAsync(
    int fileId,
    string content)
    {
        var existingSymbols = await _db.CodeSymbols
            .Where(x => x.FileId == fileId)
            .ToListAsync();

        if (existingSymbols.Count > 0)
        {
            var existingSymbolIds = existingSymbols
                .Select(x => x.Id)
                .ToList();

            var existingRelationships = await _db.CodeSymbolRelationships
                .Where(x =>
                    existingSymbolIds.Contains(x.FromSymbolId) ||
                    existingSymbolIds.Contains(x.ToSymbolId))
                .ToListAsync();

            if (existingRelationships.Count > 0)
            {
                _db.CodeSymbolRelationships.RemoveRange(
                    existingRelationships);
            }

            _db.CodeSymbols.RemoveRange(existingSymbols);
        }

        var symbols = _symbolExtractor.Extract(
            fileId,
            content);

        if (symbols.Count > 0)
        {
            _db.CodeSymbols.AddRange(symbols);
        }
    }
    public IndexService(
    AppDbContext db,
    CodeSymbolExtractor symbolExtractor,
    CodeSymbolRelationshipBuilder relationshipBuilder)
    {
        _db = db;
        _symbolExtractor = symbolExtractor;
    }

    public async Task IndexRepositoryAsync(int repositoryId)
    {
        var repository = await _db.Repositories
            .FirstOrDefaultAsync(x => x.Id == repositoryId);

        if (repository == null)
        {
            throw new InvalidOperationException(
                $"Repository with ID {repositoryId} was not found.");
        }

        if (!Directory.Exists(repository.RootPath))
        {
            throw new DirectoryNotFoundException(
                $"Repository path does not exist: {repository.RootPath}");
        }

        var filePaths = Directory
            .EnumerateFiles(
                repository.RootPath,
                "*.*",
                SearchOption.AllDirectories)
            .Where(IsSupportedFile)
            .Where(file => !IsIgnoredFile(file, repository.RootPath))
            .ToList();

        var existingFiles = await _db.Files
            .Include(x => x.CodeContent)
            .Where(x => x.RepositoryId == repositoryId)
            .ToListAsync();

        var existingFilesByPath = existingFiles
            .ToDictionary(
                x => x.RelativePath,
                StringComparer.OrdinalIgnoreCase);

        var filesFoundOnDisk = new HashSet<string>(
            StringComparer.OrdinalIgnoreCase);

        var added = 0;
        var updated = 0;
        var unchanged = 0;

        foreach (var filePath in filePaths)
        {
            var relativePath = Path.GetRelativePath(
                repository.RootPath,
                filePath);

            filesFoundOnDisk.Add(relativePath);

            var fileInfo = new FileInfo(filePath);

            string content;

            try
            {
                content = await File.ReadAllTextAsync(filePath);
            }
            catch
            {
                continue;
            }

            var contentHash = CalculateHash(content);

            if (!existingFilesByPath.TryGetValue(
                    relativePath,
                    out var existingFile))
            {
                existingFile = new FileMetadata
                {
                    RepositoryId = repositoryId,
                    FileName = fileInfo.Name,
                    Extension = fileInfo.Extension,
                    RelativePath = relativePath,
                    FullPath = fileInfo.FullName,
                    Folder = fileInfo.DirectoryName ?? string.Empty,
                    Size = fileInfo.Length,
                    LastModified = fileInfo.LastWriteTimeUtc,
                    IndexedAt = DateTime.UtcNow
                };

                _db.Files.Add(existingFile);

                await _db.SaveChangesAsync();

                var codeContent = new CodeContent
                {
                    FileId = existingFile.Id,
                    Content = content,
                    ContentHash = contentHash,
                    IndexedAt = DateTime.UtcNow
                };

                _db.CodeContents.Add(codeContent);

                await IndexSymbolsAsync(
                    existingFile.Id,
                    content);

                added++;

                continue;
            }

            if (existingFile.CodeContent?.ContentHash == contentHash)
            {
                await IndexSymbolsAsync(
                    existingFile.Id,
                    content);
 
                unchanged++;
                continue;
            }

            existingFile.FileName = fileInfo.Name;
            existingFile.Extension = fileInfo.Extension;
            existingFile.FullPath = fileInfo.FullName;
            existingFile.Folder = fileInfo.DirectoryName ?? string.Empty;
            existingFile.Size = fileInfo.Length;
            existingFile.LastModified = fileInfo.LastWriteTimeUtc;
            existingFile.IndexedAt = DateTime.UtcNow;

            if (existingFile.CodeContent == null)
            {
                existingFile.CodeContent = new CodeContent
                {
                    FileId = existingFile.Id,
                    Content = content,
                    ContentHash = contentHash,
                    IndexedAt = DateTime.UtcNow
                };
            }
            else
            {
                existingFile.CodeContent.Content = content;
                existingFile.CodeContent.ContentHash = contentHash;
                existingFile.CodeContent.IndexedAt = DateTime.UtcNow;

                await IndexSymbolsAsync(
                    existingFile.Id,
                    content);

            }

            updated++;
        }

        var deletedFiles = existingFiles
    .Where(x => !filesFoundOnDisk.Contains(x.RelativePath))
    .ToList();

        foreach (var deletedFile in deletedFiles)
        {
            var deletedSymbolIds = await _db.CodeSymbols
                .Where(x => x.FileId == deletedFile.Id)
                .Select(x => x.Id)
                .ToListAsync();

            if (deletedSymbolIds.Count > 0)
            {
                var relationships = await _db.CodeSymbolRelationships
                    .Where(x =>
                        deletedSymbolIds.Contains(x.FromSymbolId) ||
                        deletedSymbolIds.Contains(x.ToSymbolId))
                    .ToListAsync();

                if (relationships.Count > 0)
                {
                    _db.CodeSymbolRelationships.RemoveRange(
                        relationships);
                }
            }

            _db.Files.Remove(deletedFile);
        }

        await _db.SaveChangesAsync();

        await _db.SaveChangesAsync();
        repository.LastIndexedAt = DateTime.UtcNow;

        repository.FileCount = await _db.Files
            .CountAsync(x => x.RepositoryId == repositoryId);

        await _db.SaveChangesAsync();

        Console.WriteLine(
            $"Repository '{repository.Name}' indexed. " +
            $"Added: {added}, " +
            $"Updated: {updated}, " +
            $"Unchanged: {unchanged}, " +
            $"Deleted: {deletedFiles.Count}");
    }

    private static bool IsSupportedFile(string filePath)
    {
        return SupportedExtensions.Contains(
            Path.GetExtension(filePath));
    }

    private static bool IsIgnoredFile(
        string filePath,
        string repositoryRoot)
    {
        var relativePath = Path.GetRelativePath(
            repositoryRoot,
            filePath);

        var directories = relativePath.Split(
            Path.DirectorySeparatorChar,
            Path.AltDirectorySeparatorChar);

        return directories.Any(IgnoredDirectories.Contains);
    }

    private static string CalculateHash(string content)
    {
        var bytes = Encoding.UTF8.GetBytes(content);

        var hash = SHA256.HashData(bytes);

        return Convert.ToHexString(hash);
    }


}