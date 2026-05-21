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
        if (string.IsNullOrWhiteSpace(request.HobbyDescription))
        {
            return Results.BadRequest(new ProblemDetailsResponse("Hobby description is required."));
        }

        if (string.IsNullOrWhiteSpace(request.Postcode))
        {
            return Results.BadRequest(new ProblemDetailsResponse("UK postcode is required."));
        }

        if (request.MaximumDistanceMiles <= 0)
        {
            return Results.BadRequest(new ProblemDetailsResponse("Maximum distance must be greater than zero."));
        }

        if (request.MaximumDistanceMiles > 100)
        {
            return Results.BadRequest(new ProblemDetailsResponse("Maximum distance must be 100 miles or less."));
        }

        var openAiApiKey = configuration["OPENAI_API_KEY"];

        if (string.IsNullOrWhiteSpace(openAiApiKey))
        {
            return Results.Problem(
                detail: "OpenAI API key is not configured.",
                statusCode: StatusCodes.Status500InternalServerError);
        }

        var prompt = $$"""
                       Find available UK courses and classes for the following hobby.

                       Hobby description:
                       {{request.HobbyDescription}}

                       UK postcode:
                       {{request.Postcode}}

                       Maximum distance:
                       {{request.MaximumDistanceMiles}} miles

                       Requirements:
                       - Search for real, currently available courses/classes/workshops.
                       - Prefer official provider pages.
                       - Only include results that appear to be within the requested distance of the postcode.
                       - Return up to 10 results.
                       - If exact distance is uncertain, estimate it and set "distanceIsEstimated" to true.
                       - Include booking/contact URL where available.
                       - Do not invent providers, URLs, prices, or dates.
                       - If there are not enough results, return fewer results.
                       - Return JSON only, with this exact shape:

                       {
                         "query": {
                           "hobbyDescription": "...",
                           "postcode": "...",
                           "maximumDistanceMiles": 10
                         },
                         "results": [
                           {
                             "title": "...",
                             "providerName": "...",
                             "description": "...",
                             "address": "...",
                             "postcode": "...",
                             "estimatedDistanceMiles": 1.2,
                             "distanceIsEstimated": true,
                             "price": "...",
                             "schedule": "...",
                             "bookingUrl": "...",
                             "sourceUrl": "..."
                           }
                         ],
                         "notes": "..."
                       }
                       """;

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
