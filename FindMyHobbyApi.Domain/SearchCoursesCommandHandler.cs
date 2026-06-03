using System.Text.Json;

namespace FindMyHobbyApi.Domain;

public sealed class SearchCoursesCommandHandler : ISearchCoursesCommandHandler
{
    private readonly ICourseSearchClient _courseSearchClient;

    public SearchCoursesCommandHandler(ICourseSearchClient courseSearchClient)
    {
        _courseSearchClient = courseSearchClient;
    }

    public async Task<SearchCoursesOutcome> HandleAsync(SearchCoursesCommand command, CancellationToken cancellationToken)
    {
        var validationError = CourseSearchGuardrails.Validate(command.Request);
        if (validationError is not null)
        {
            return SearchCoursesOutcome.ValidationError(validationError);
        }

        var prompt = CourseSearchGuardrails.BuildPrompt(command.Request);

        string outputText;
        try
        {
            outputText = await _courseSearchClient.SearchAsync(prompt, cancellationToken);
        }
        catch (Exception ex)
        {
            return SearchCoursesOutcome.UpstreamFailure(ex.Message);
        }

        if (string.IsNullOrWhiteSpace(outputText))
        {
            return SearchCoursesOutcome.UpstreamFailure("OpenAI returned an empty response.");
        }

        try
        {
            var courseSearchResponse = JsonSerializer.Deserialize<CourseSearchResponse>(outputText, JsonOptions.Default);

            return courseSearchResponse is null
                ? SearchCoursesOutcome.UpstreamFailure("Unable to parse OpenAI response.")
                : SearchCoursesOutcome.Success(courseSearchResponse);
        }
        catch (JsonException)
        {
            return SearchCoursesOutcome.Raw(new CourseSearchRawResponse(outputText));
        }
    }
}
