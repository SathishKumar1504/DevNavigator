using DevNavigator.Api.Data;
using DevNavigator.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace DevNavigator.Api.Services;

public class ImportResolver
{
    private readonly AppDbContext _db;

    public ImportResolver(AppDbContext db)
    {
        _db = db;
    }

    public async Task<FileMetadata?> ResolveAsync(
        int repositoryId,
        string importPath,
        string importingFilePath)
    {
        if (string.IsNullOrWhiteSpace(importPath))
            return null;

        var normalizedImport = importPath
            .Replace("\\", "/")
            .Trim();

        string normalizedPath;

        // @/ means src/
        if (normalizedImport.StartsWith("@/"))
        {
            normalizedPath =
                "src/" + normalizedImport.Substring(2);
        }
        // Relative import
        else if (
            normalizedImport.StartsWith("./") ||
            normalizedImport.StartsWith("../"))
        {
            normalizedPath = ResolveRelativePath(
                importingFilePath,
                normalizedImport);
        }
        else
        {
            // npm package such as react, lodash-es, etc.
            return null;
        }

        var possiblePaths = new List<string>
        {
            normalizedPath,
            normalizedPath + ".js",
            normalizedPath + ".jsx",
            normalizedPath + ".ts",
            normalizedPath + ".tsx",
            normalizedPath + "/index.js",
            normalizedPath + "/index.jsx",
            normalizedPath + "/index.ts",
            normalizedPath + "/index.tsx"
        };

        return await _db.Files
            .Where(x =>
                x.RepositoryId == repositoryId &&
                possiblePaths.Contains(
                    x.RelativePath.Replace("\\", "/")))
            .FirstOrDefaultAsync();
    }

    private static string ResolveRelativePath(
        string importingFilePath,
        string importPath)
    {
        var filePath = importingFilePath
            .Replace("\\", "/");

        var directory = Path.GetDirectoryName(filePath)
            ?.Replace("\\", "/")
            ?? string.Empty;

        var combinedPath =
            $"{directory}/{importPath}";

        var segments = combinedPath
            .Split(
                '/',
                StringSplitOptions.RemoveEmptyEntries);

        var resolved = new List<string>();

        foreach (var segment in segments)
        {
            if (segment == ".")
            {
                continue;
            }

            if (segment == "..")
            {
                if (resolved.Count > 0)
                {
                    resolved.RemoveAt(
                        resolved.Count - 1);
                }

                continue;
            }

            resolved.Add(segment);
        }

        return string.Join("/", resolved);
    }
}