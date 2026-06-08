namespace FindMyHobbyApi.Domain.Models;

public sealed record CourseSearchRequest(
    string HobbyDescription,
    string Postcode,
    int MaximumDistanceMiles);
