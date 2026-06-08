namespace FindMyHobbyApi.Domain.Models;

public sealed record CourseSearchQuery(
    string HobbyDescription,
    string Postcode,
    int MaximumDistanceMiles);
