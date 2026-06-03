namespace FindMyHobbyApi.Domain;

public interface ICourseSearchClient
{
    Task<string> SearchAsync(string prompt, CancellationToken cancellationToken);
}
