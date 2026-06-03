namespace FindMyHobbyApi.Domain;

public interface ISearchCoursesCommandHandler
{
    Task<SearchCoursesOutcome> HandleAsync(SearchCoursesCommand command, CancellationToken cancellationToken);
}
