using FindMyHobbyApi.Domain.Models;

namespace FindMyHobbyApi.Domain.Handlers.Queries;

public interface IGetHobbyQueryHandler
{
    Task<Hobby[]> HandleAsync(GetHobbyQuery query, CancellationToken cancellationToken);
}
