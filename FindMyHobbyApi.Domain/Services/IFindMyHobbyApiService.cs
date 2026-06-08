using FindMyHobbyApi.Domain.Models;

namespace FindMyHobbyApi.Domain.Services;

public interface IFindMyHobbyApiService
{
    Task<Hobby[]> GetHobbyAsync(CancellationToken cancellationToken);
    Task<SearchCoursesOutcome> SearchCoursesAsync(CourseSearchRequest request, CancellationToken cancellationToken);
}
