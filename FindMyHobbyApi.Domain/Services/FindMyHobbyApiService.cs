using FindMyHobbyApi.Domain.Handlers.Commands;
using FindMyHobbyApi.Domain.Handlers.Queries;
using FindMyHobbyApi.Domain.Models;

namespace FindMyHobbyApi.Domain.Services;

public sealed class FindMyHobbyApiService(
    IGetHobbyQueryHandler getHobbyQueryHandler,
    ISearchCoursesCommandHandler searchCoursesCommandHandler)
    : IFindMyHobbyApiService
{
    public Task<Hobby[]> GetHobbyAsync(CancellationToken cancellationToken)
        => getHobbyQueryHandler.HandleAsync(new GetHobbyQuery(), cancellationToken);

    public Task<SearchCoursesOutcome> SearchCoursesAsync(CourseSearchRequest request, CancellationToken cancellationToken)
        => searchCoursesCommandHandler.HandleAsync(new SearchCoursesCommand(request), cancellationToken);
}
