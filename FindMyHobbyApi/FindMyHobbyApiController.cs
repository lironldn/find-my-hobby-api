using FindMyHobbyApi.Domain;

namespace FindMyHobbyApi;

public sealed class FindMyHobbyApiController(IFindMyHobbyApiService service)
{
    public async Task<IResult> GetHobbyAsync(CancellationToken cancellationToken)
    {
        var hobbies = await service.GetHobbyAsync(cancellationToken);
        return Results.Ok(hobbies);
    }

    public async Task<IResult> SearchCoursesAsync(CourseSearchRequest request, CancellationToken cancellationToken)
    {
        var outcome = await service.SearchCoursesAsync(request, cancellationToken);

        return outcome.Kind switch
        {
            SearchCoursesOutcomeKind.ValidationError => Results.BadRequest(new ProblemDetailsResponse(outcome.Detail ?? "Invalid request.")),
            SearchCoursesOutcomeKind.UpstreamFailure => Results.Problem(
                detail: outcome.Detail,
                statusCode: StatusCodes.Status502BadGateway,
                title: "OpenAI request failed."),
            SearchCoursesOutcomeKind.RawResponse => Results.Ok(outcome.RawResponse),
            SearchCoursesOutcomeKind.Success => Results.Ok(outcome.Response),
            _ => Results.Problem(statusCode: StatusCodes.Status500InternalServerError)
        };
    }
}
