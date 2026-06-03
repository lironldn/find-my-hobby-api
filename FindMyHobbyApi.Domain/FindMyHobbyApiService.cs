namespace FindMyHobbyApi.Domain;

public sealed class FindMyHobbyApiService : IFindMyHobbyApiService
{
    private readonly IGetHobbyQueryHandler _getHobbyQueryHandler;
    private readonly ISearchCoursesCommandHandler _searchCoursesCommandHandler;

    public FindMyHobbyApiService(
        IGetHobbyQueryHandler getHobbyQueryHandler,
        ISearchCoursesCommandHandler searchCoursesCommandHandler)
    {
        _getHobbyQueryHandler = getHobbyQueryHandler;
        _searchCoursesCommandHandler = searchCoursesCommandHandler;
    }

    public Task<Hobby[]> GetHobbyAsync(CancellationToken cancellationToken)
        => _getHobbyQueryHandler.HandleAsync(new GetHobbyQuery(), cancellationToken);

    public Task<SearchCoursesOutcome> SearchCoursesAsync(CourseSearchRequest request, CancellationToken cancellationToken)
        => _searchCoursesCommandHandler.HandleAsync(new SearchCoursesCommand(request), cancellationToken);
}
