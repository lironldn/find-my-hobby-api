namespace FindMyHobbyApi.Domain;

public sealed record CourseSearchResponse(
    CourseSearchQuery Query,
    CourseSearchResult[] Results,
    string? Notes);
