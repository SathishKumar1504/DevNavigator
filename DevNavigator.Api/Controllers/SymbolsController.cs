using DevNavigator.Api.Data;
using DevNavigator.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DevNavigator.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SymbolsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly CodeSymbolExtractor _extractor;
    private readonly CodeSymbolRelationshipBuilder _relationshipBuilder;

    public SymbolsController(
        AppDbContext db,
        CodeSymbolExtractor extractor,
        CodeSymbolRelationshipBuilder relationshipBuilder)
    {
        _db = db;
        _extractor = extractor;
        _relationshipBuilder = relationshipBuilder;
    }

    [HttpPost("index/{repositoryId:int}")]
    public async Task<IActionResult> IndexSymbols(
        int repositoryId)
    {
        var files = await _db.Files
            .Include(x => x.CodeContent)
            .Where(x =>
                x.RepositoryId == repositoryId &&
                x.CodeContent != null)
            .ToListAsync();

        var indexed = 0;

        foreach (var file in files)
        {
            var existingSymbols = await _db.CodeSymbols
                .Where(x => x.FileId == file.Id)
                .ToListAsync();

            _db.CodeSymbols.RemoveRange(existingSymbols);

            var symbols = _extractor.Extract(
                file.Id,
                file.CodeContent!.Content);

            _db.CodeSymbols.AddRange(symbols);

            indexed++;
        }

        await _db.SaveChangesAsync();

        // Build relationships after all symbols have been saved
        await _relationshipBuilder.BuildForRepositoryAsync(repositoryId);

        return Ok(new
        {
            repositoryId,
            filesIndexed = indexed,

            symbols = await _db.CodeSymbols
                .CountAsync(x =>
                    x.File!.RepositoryId == repositoryId),

            relationships = await _db.CodeSymbolRelationships
                .CountAsync(x =>
                    x.FromSymbol!.File!.RepositoryId == repositoryId)
        });
    }
    [HttpGet("file/{fileId:int}")]
    public async Task<IActionResult> GetFileSymbols(
    int fileId)
    {
        var symbols = await _db.CodeSymbols
            .Where(x => x.FileId == fileId)
            .OrderBy(x => x.LineNumber)
            .Select(x => new
            {
                x.Id,
                x.SymbolType,
                x.Name,
                x.LineNumber,
                x.ImportPath
            })
            .ToListAsync();

        return Ok(symbols);
    }

    [HttpGet("relationships/file/{fileId:int}")]
    public async Task<IActionResult> GetFileRelationships(int fileId)
    {
        var relationships = await _db.CodeSymbolRelationships
            .Where(x =>
                x.FromSymbol!.FileId == fileId ||
                x.ToSymbol!.FileId == fileId)
            .Select(x => new
            {
                relationshipId = x.Id,
                relationshipType = x.RelationshipType,

                from = new
                {
                    symbolId = x.FromSymbolId,
                    name = x.FromSymbol!.Name,
                    symbolType = x.FromSymbol.SymbolType,
                    fileId = x.FromSymbol.FileId
                },

                to = new
                {
                    symbolId = x.ToSymbolId,
                    name = x.ToSymbol!.Name,
                    symbolType = x.ToSymbol.SymbolType,
                    fileId = x.ToSymbol.FileId
                }
            })
            .ToListAsync();

        return Ok(relationships);
    }
}