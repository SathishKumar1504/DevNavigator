namespace DevNavigator.Api.Services;

public interface INavigationService
{
    Task<object?> GetFileNavigationAsync(
        int fileId,
        int depth);
}