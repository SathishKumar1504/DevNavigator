using DevNavigator.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace DevNavigator.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SearchController : ControllerBase
{
    private readonly ISearchService _searchService;

    public SearchController(ISearchService searchService)
    {
        _searchService = searchService;
    }

    [HttpGet]
    public async Task<IActionResult> Search(
        [FromQuery] int repositoryId,
        [FromQuery] string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return BadRequest("Search query is required.");
        }

        var results = await _searchService.SearchAsync(
            repositoryId,
            query.Trim());

        return Ok(new
        {
            query,
            count = results.Count,
            results
        });
    }
}