namespace FindMyHobbyApi.Domain;

public sealed class GetHobbyQueryHandler : IGetHobbyQueryHandler
{
    private static readonly string[] Hobbies =
    [
        "Hiking",
        "Bouldering",
        "Karaoke",
        "Pottery",
        "Cooking",
        "Drawing",
        "Salsa dancing",
        "Art Galleries",
        "Cinema",
        "Theatre"
    ];

    public Task<Hobby[]> HandleAsync(GetHobbyQuery query, CancellationToken cancellationToken)
    {
        var hobbies = Enumerable.Range(1, 5)
            .Select(_ => new Hobby(Hobbies[Random.Shared.Next(Hobbies.Length)]))
            .ToArray();

        return Task.FromResult(hobbies);
    }
}
