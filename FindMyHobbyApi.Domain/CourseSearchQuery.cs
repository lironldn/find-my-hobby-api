namespace FindMyHobbyApi.Domain;

public sealed record CourseSearchQuery(
    string HobbyDescription,
    string Postcode,
    int MaximumDistanceMiles);
