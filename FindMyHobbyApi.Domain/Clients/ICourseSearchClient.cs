namespace FindMyHobbyApi.Domain.Clients;

public interface ICourseSearchClient
{
    Task<string> SearchAsync(string prompt, CancellationToken cancellationToken);
}
