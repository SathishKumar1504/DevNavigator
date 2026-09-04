using DevNavigator.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace DevNavigator.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class IndexController : ControllerBase
{
    private readonly IIndexService _indexService;
    private readonly CodeSymbolRelationshipBuilder _relationshipBuilder;

    public IndexController(
        IIndexService indexService,
        CodeSymbolRelationshipBuilder relationshipBuilder)
    {
        _indexService = indexService;
        _relationshipBuilder = relationshipBuilder;
    }

    [HttpPost("{repositoryId:int}")]
    public async Task<IActionResult> Index(int repositoryId)
    {
        try
        {
            await _indexService.IndexRepositoryAsync(repositoryId);

            await _relationshipBuilder.BuildForRepositoryAsync(repositoryId);

            return Ok(new
            {
                message = "Repository indexed successfully.",
                repositoryId
            });
        }
        catch (DirectoryNotFoundException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new
            {
                message = ex.Message
            });
        }
    }
}