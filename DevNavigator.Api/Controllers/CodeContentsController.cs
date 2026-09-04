using DevNavigator.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DevNavigator.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CodeContentsController : ControllerBase
{
    private readonly AppDbContext _db;

    public CodeContentsController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet("{fileId:int}")]
    public async Task<IActionResult> GetContent(int fileId)
    {
        var content = await _db.CodeContents
            .FirstOrDefaultAsync(x => x.FileId == fileId);

        if (content == null)
        {
            return NotFound();
        }

        return Ok(new
        {
            content.FileId,
            content.ContentHash,
            content.IndexedAt,
            content.Content
        });
    }
}