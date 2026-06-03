namespace FindMyHobbyApi.Domain;

public interface IGetHobbyQueryHandler
{
    Task<Hobby[]> HandleAsync(GetHobbyQuery query, CancellationToken cancellationToken);
}
