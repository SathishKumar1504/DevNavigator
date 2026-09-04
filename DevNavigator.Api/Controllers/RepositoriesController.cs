using DevNavigator.Api.Data;
using DevNavigator.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DevNavigator.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RepositoriesController : ControllerBase
{
    private readonly AppDbContext _db;

    public RepositoriesController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetRepositories()
    {
        var repositories = await _db.Repositories
            .OrderBy(x => x.Name)
            .ToListAsync();

        return Ok(repositories);
    }

    [HttpPost]
    public async Task<IActionResult> AddRepository(Repository repository)
    {
        if (string.IsNullOrWhiteSpace(repository.RootPath))
        {
            return BadRequest("Repository path is required.");
        }

        if (!Directory.Exists(repository.RootPath))
        {
            return BadRequest("Repository path does not exist.");
        }

        repository.Name = string.IsNullOrWhiteSpace(repository.Name)
            ? new DirectoryInfo(repository.RootPath).Name
            : repository.Name;

        repository.AddedAt = DateTime.UtcNow;

        _db.Repositories.Add(repository);

        await _db.SaveChangesAsync();

        return Ok(repository);
    }
}