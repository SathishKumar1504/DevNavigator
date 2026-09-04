using DevNavigator.Api.Data;
using DevNavigator.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DevNavigator.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NavigationController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ImportResolver _resolver;
    private readonly ServiceNavigationService _serviceNavigation;

    public NavigationController(
    AppDbContext db,
    ImportResolver resolver,
    ServiceNavigationService serviceNavigation)
    {
        _db = db;
        _resolver = resolver;
        _serviceNavigation = serviceNavigation;
    }

    [HttpGet("file/{fileId:int}")]
    public async Task<IActionResult> GetFileNavigation(
        int fileId,
        [FromQuery] int depth = 1)
    {
        depth = Math.Clamp(depth, 1, 5);

        var visited = new HashSet<int>();

        var result = await BuildNavigationTree(
            fileId,
            depth,
            visited);

        if (result == null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    private async Task<object?> BuildNavigationTree(
        int fileId,
        int depth,
        HashSet<int> visited)
    {
        if (!visited.Add(fileId))
        {
            return null;
        }

        var file = await _db.Files
            .FirstOrDefaultAsync(x => x.Id == fileId);

        if (file == null)
        {
            return null;
        }

        var imports = await _db.CodeSymbols
            .Where(x =>
                x.FileId == fileId &&
                x.SymbolType == "Import" &&
                x.ImportPath != null)
            .OrderBy(x => x.LineNumber)
            .ToListAsync();

        var dependencies = new List<object>();

        foreach (var import in imports)
        {
            var resolvedFile = await _resolver.ResolveAsync(
                file.RepositoryId,
                import.ImportPath!,
                file.RelativePath);

            // Follow index.js / barrel file to actual implementation.
            if (resolvedFile != null)
            {
                resolvedFile = await ResolveBarrelFile(
                    file.RepositoryId,
                    resolvedFile);
            }

            object? children = null;

            if (resolvedFile != null && depth > 1)
            {
                children = await BuildNavigationTree(
                    resolvedFile.Id,
                    depth - 1,
                    new HashSet<int>(visited));
            }

            dependencies.Add(new
            {
                symbol = import.Name,
                importPath = import.ImportPath,
                lineNumber = import.LineNumber,
                resolved = resolvedFile != null,
                fileId = resolvedFile?.Id,
                fileName = resolvedFile?.FileName,
                relativePath = resolvedFile?.RelativePath,
                children
            });
        }

        return new
        {
            fileId = file.Id,
            fileName = file.FileName,
            relativePath = file.RelativePath,
            dependencies
        };
    }

    private async Task<Models.FileMetadata> ResolveBarrelFile(
        int repositoryId,
        Models.FileMetadata file)
    {
        var currentFile = file;

        var visited = new HashSet<int>();

        while (visited.Add(currentFile.Id))
        {
            var exports = await _db.CodeSymbols
                .Where(x =>
                    x.FileId == currentFile.Id &&
                    x.SymbolType == "Export" &&
                    x.ImportPath != null)
                .OrderBy(x => x.LineNumber)
                .ToListAsync();

            if (exports.Count == 0)
            {
                return currentFile;
            }

            var export = exports.First();

            var targetFile = await _resolver.ResolveAsync(
                repositoryId,
                export.ImportPath!,
                currentFile.RelativePath);

            if (targetFile == null)
            {
                return currentFile;
            }

            currentFile = targetFile;
        }

        return currentFile;
    }

    [HttpGet("service/{fileId:int}/{symbol}")]
    public async Task<IActionResult> GetServiceNavigation(
    int fileId,
    string symbol)
    {
        var file = await _db.Files
            .FirstOrDefaultAsync(x => x.Id == fileId);

        if (file == null)
        {
            return NotFound();
        }

        var result = await _serviceNavigation.ResolveServiceAsync(
            file.RepositoryId,
            fileId,
            symbol);

        if (result == null)
        {
            return NotFound(new
            {
                message = $"Could not resolve service '{symbol}'."
            });
        }

        return Ok(result);
    }

    [HttpGet("symbol/{symbolId:int}")]
    public async Task<IActionResult> GetSymbolNavigation(
    int symbolId)
    {
        var symbol = await _db.CodeSymbols
            .Include(x => x.File)
            .FirstOrDefaultAsync(x => x.Id == symbolId);

        if (symbol == null)
        {
            return NotFound(new
            {
                message = $"Symbol with ID {symbolId} was not found."
            });
        }

        var outgoing = await _db.CodeSymbolRelationships
            .Where(x => x.FromSymbolId == symbolId)
            .Include(x => x.ToSymbol)
            .ThenInclude(x => x.File)
            .Select(x => new
            {
                relationshipType = x.RelationshipType,
                symbolId = x.ToSymbolId,
                name = x.ToSymbol!.Name,
                symbolType = x.ToSymbol.SymbolType,
                fileId = x.ToSymbol.FileId,
                fileName = x.ToSymbol.File!.FileName,
                relativePath = x.ToSymbol.File.RelativePath,
                lineNumber = x.ToSymbol.LineNumber
            })
            .ToListAsync();

        var incoming = await _db.CodeSymbolRelationships
            .Where(x => x.ToSymbolId == symbolId)
            .Include(x => x.FromSymbol)
            .ThenInclude(x => x.File)
            .Select(x => new
            {
                relationshipType = x.RelationshipType,
                symbolId = x.FromSymbolId,
                name = x.FromSymbol!.Name,
                symbolType = x.FromSymbol.SymbolType,
                fileId = x.FromSymbol.FileId,
                fileName = x.FromSymbol.File!.FileName,
                relativePath = x.FromSymbol.File.RelativePath,
                lineNumber = x.FromSymbol.LineNumber
            })
            .ToListAsync();

        return Ok(new
        {
            symbol = new
            {
                id = symbol.Id,
                name = symbol.Name,
                symbolType = symbol.SymbolType,
                fileId = symbol.FileId,
                fileName = symbol.File!.FileName,
                relativePath = symbol.File.RelativePath,
                lineNumber = symbol.LineNumber
            },
            outgoing,
            incoming
        });
    }
    [HttpGet("symbol/{symbolId:int}/graph")]
    public async Task<IActionResult> GetSymbolGraph(
    int symbolId,
    [FromQuery] int depth = 3)
    {
        depth = Math.Clamp(depth, 1, 5);

        var symbol = await _db.CodeSymbols
            .Include(x => x.File)
            .FirstOrDefaultAsync(x => x.Id == symbolId);

        if (symbol == null)
        {
            return NotFound(new
            {
                message = $"Symbol with ID {symbolId} was not found."
            });
        }

        var visited = new HashSet<int>();

        var graph = await BuildSymbolGraph(
            symbolId,
            depth,
            visited);

        return Ok(graph);
    }

 
    private async Task<object?> BuildSymbolGraph(
        int symbolId,
        int depth,
        HashSet<int> visited)
    {
        if (depth <= 0)
            return null;

        if (!visited.Add(symbolId))
            return null;

        var symbol = await _db.CodeSymbols
            .Include(x => x.File)
            .FirstOrDefaultAsync(x => x.Id == symbolId);

        if (symbol == null)
            return null;

        var relationships = await _db.CodeSymbolRelationships
            .Where(x => x.FromSymbolId == symbolId)
            .Include(x => x.ToSymbol)
            .ThenInclude(x => x.File)
            .Select(x => new
            {
                relationshipId = x.Id,
                relationshipType = x.RelationshipType,
                symbol = new
                {
                    id = x.ToSymbolId,
                    name = x.ToSymbol!.Name,
                    symbolType = x.ToSymbol.SymbolType,
                    fileId = x.ToSymbol.FileId,
                    fileName = x.ToSymbol.File!.FileName,
                    relativePath = x.ToSymbol.File.RelativePath,
                    lineNumber = x.ToSymbol.LineNumber
                }
            })
            .ToListAsync();

        var children = new List<object>();

        foreach (var relationship in relationships)
        {
            var childVisited = new HashSet<int>(visited);

            var child = await BuildSymbolGraph(
                relationship.symbol.id,
                depth - 1,
                childVisited);

            children.Add(new
            {
                relationshipId = relationship.relationshipId,
                relationshipType = relationship.relationshipType,
                symbol = relationship.symbol,
                children = child
            });
        }

        return new
        {
            symbol = new
            {
                id = symbol.Id,
                name = symbol.Name,
                symbolType = symbol.SymbolType,
                fileId = symbol.FileId,
                fileName = symbol.File!.FileName,
                relativePath = symbol.File.RelativePath,
                lineNumber = symbol.LineNumber
            },
            children
        };
    }


}