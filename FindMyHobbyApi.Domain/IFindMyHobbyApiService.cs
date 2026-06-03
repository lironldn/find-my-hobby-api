namespace FindMyHobbyApi.Domain;

public interface IFindMyHobbyApiService
{
    Task<Hobby[]> GetHobbyAsync(CancellationToken cancellationToken);
    Task<SearchCoursesOutcome> SearchCoursesAsync(CourseSearchRequest request, CancellationToken cancellationToken);
}
