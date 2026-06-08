using FindMyHobbyApi.Domain.Models;

namespace FindMyHobbyApi.Domain.Handlers.Commands;

public interface ISearchCoursesCommandHandler
{
    Task<SearchCoursesOutcome> HandleAsync(SearchCoursesCommand command, CancellationToken cancellationToken);
}
