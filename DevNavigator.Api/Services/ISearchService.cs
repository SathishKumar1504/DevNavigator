using DevNavigator.Api.DTOs;

namespace DevNavigator.Api.Services;

public interface ISearchService
{
    Task<List<SearchResultDto>> SearchAsync(
        int repositoryId,
        string query);
}