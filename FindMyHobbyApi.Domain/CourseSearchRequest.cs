namespace FindMyHobbyApi.Domain;

public sealed record CourseSearchRequest(
    string HobbyDescription,
    string Postcode,
    int MaximumDistanceMiles);
