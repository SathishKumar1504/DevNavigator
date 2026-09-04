namespace DevNavigator.Api.Services;

public interface IIndexService
{
    Task IndexRepositoryAsync(int repositoryId);
}