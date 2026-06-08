namespace FindMyHobbyApi.Domain.Models;

public enum SearchCoursesOutcomeKind
{
    Success,
    RawResponse,
    ValidationError,
    UpstreamFailure
}

public sealed record SearchCoursesOutcome(
    SearchCoursesOutcomeKind Kind,
    CourseSearchResponse? Response = null,
    CourseSearchRawResponse? RawResponse = null,
    string? Detail = null)
{
    public static SearchCoursesOutcome Success(CourseSearchResponse response)
        => new(SearchCoursesOutcomeKind.Success, Response: response);

    public static SearchCoursesOutcome Raw(CourseSearchRawResponse rawResponse)
        => new(SearchCoursesOutcomeKind.RawResponse, RawResponse: rawResponse);

    public static SearchCoursesOutcome ValidationError(string detail)
        => new(SearchCoursesOutcomeKind.ValidationError, Detail: detail);

    public static SearchCoursesOutcome UpstreamFailure(string detail)
        => new(SearchCoursesOutcomeKind.UpstreamFailure, Detail: detail);
}
