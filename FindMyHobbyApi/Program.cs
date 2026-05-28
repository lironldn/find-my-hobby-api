using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);

builder.Services.AddOpenApi();

builder.Services.AddHttpClient("OpenAI", client =>
{
    client.BaseAddress = new Uri("https://api.openai.com/");
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

var useHttpsRedirection = builder.Configuration.GetValue("UseHttpsRedirection", false);
if (useHttpsRedirection)
{
    app.UseHttpsRedirection();
}

var hobbies = new[]
{
    "Hiking", "Bouldering", "Karaoke", "Pottery", "Cooking", "Drawing", "Salsa dancing", "Art Galleries", "Cinema", "Theatre"
};

app.MapGet("/hobby", () =>
    {
        var hobby = Enumerable.Range(1, 5).Select(index =>
                new Hobby
                (
                    hobbies[Random.Shared.Next(hobbies.Length)]
                ))
            .ToArray();
        return hobby;
    })
    .WithName("GetHobby");

app.MapPost("/courses/search", async (
        CourseSearchRequest request,
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        CancellationToken cancellationToken) =>
    {
        var validationError = CourseSearchGuardrails.Validate(request);
        if (validationError is not null)
        {
            return Results.BadRequest(new ProblemDetailsResponse(validationError));
        }

        var openAiApiKey = configuration["OPENAI_API_KEY"];

        if (string.IsNullOrWhiteSpace(openAiApiKey))
        {
            return Results.Problem(
                detail: "OpenAI API key is not configured.",
                statusCode: StatusCodes.Status500InternalServerError);
        }

        var prompt = CourseSearchGuardrails.BuildPrompt(request);

        var openAiRequest = new OpenAiResponsesRequest(
            Model: "gpt-4.1",
            Tools:
            [
                new OpenAiTool("web_search_preview")
            ],
            Input: prompt
        );

        var httpClient = httpClientFactory.CreateClient("OpenAI");
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", openAiApiKey);

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "v1/responses")
        {
            Content = new StringContent(
                JsonSerializer.Serialize(openAiRequest, JsonOptions.Default),
                Encoding.UTF8,
                "application/json")
        };

        using var response = await httpClient.SendAsync(httpRequest, cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return Results.Problem(
                detail: responseBody,
                statusCode: StatusCodes.Status502BadGateway,
                title: "OpenAI request failed.");
        }

        var openAiResponse = JsonSerializer.Deserialize<OpenAiResponsesResponse>(responseBody, JsonOptions.Default);
        var outputText = openAiResponse?.OutputText;

        if (string.IsNullOrWhiteSpace(outputText))
        {
            return Results.Problem(
                detail: "OpenAI returned an empty response.",
                statusCode: StatusCodes.Status502BadGateway);
        }

        try
        {
            var courseSearchResponse = JsonSerializer.Deserialize<CourseSearchResponse>(outputText, JsonOptions.Default);

            return courseSearchResponse is null
                ? Results.Problem("Unable to parse OpenAI response.")
                : Results.Ok(courseSearchResponse);
        }
        catch (JsonException)
        {
            return Results.Ok(new CourseSearchRawResponse(outputText));
        }
    })
    .WithName("SearchCourses");

app.MapGet("/health", () => Results.Ok(new { status = "Healthy" }))
    .WithName("Health");

app.Run();
