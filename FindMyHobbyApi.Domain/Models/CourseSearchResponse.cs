namespace FindMyHobbyApi.Domain.Models;

public sealed record CourseSearchResponse(
    CourseSearchQuery Query,
    CourseSearchResult[] Results,
    string? Notes);
