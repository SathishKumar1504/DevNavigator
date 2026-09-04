using DevNavigator.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DevNavigator.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FilesController : ControllerBase
{
    private readonly AppDbContext _db;

    public FilesController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetFiles(
        [FromQuery] int repositoryId)
    {
        var files = await _db.Files
            .Where(x => x.RepositoryId == repositoryId)
            .OrderBy(x => x.RelativePath)
            .Select(x => new
            {
                x.Id,
                x.FileName,
                x.Extension,
                x.RelativePath,
                x.Size,
                x.LastModified,
                x.IndexedAt
            })
            .ToListAsync();

        return Ok(files);
    }
}