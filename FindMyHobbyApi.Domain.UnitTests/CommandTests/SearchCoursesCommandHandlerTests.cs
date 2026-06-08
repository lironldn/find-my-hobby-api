using FluentAssertions;
using FindMyHobbyApi.Domain.Clients;
using FindMyHobbyApi.Domain.Handlers.Commands;
using FindMyHobbyApi.Domain.Models;
using NSubstitute;

namespace FindMyHobbyApi.Domain.UnitTests.CommandTests;

public sealed class SearchCoursesCommandHandlerTests
{
    [Test]
    public async Task HandleAsync_returns_validation_error_without_calling_client_for_bad_input()
    {
        var client = Substitute.For<ICourseSearchClient>();
        var handler = new SearchCoursesCommandHandler(client);

        var result = await handler.HandleAsync(new SearchCoursesCommand(new CourseSearchRequest("", "SW1A 1AA", 10)), CancellationToken.None);

        result.Kind.Should().Be(SearchCoursesOutcomeKind.ValidationError);
        result.Detail.Should().Be("Hobby description is required.");
        await client.DidNotReceiveWithAnyArgs().SearchAsync(default!, default);
    }

    [Test]
    public async Task HandleAsync_returns_parsed_response_for_valid_json()
    {
        var client = Substitute.For<ICourseSearchClient>();
        client.SearchAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult("""
            {
              "query": {
                "hobbyDescription": "pottery",
                "postcode": "SW1A 1AA",
                "maximumDistanceMiles": 10
              },
              "results": [
                {
                  "title": "Beginner Pottery",
                  "providerName": "Studio One",
                  "description": "A class for new potters.",
                  "address": "10 Example Road",
                  "postcode": "SW1A 1AA",
                  "estimatedDistanceMiles": 1.5,
                  "distanceIsEstimated": true,
                  "price": "From £25",
                  "schedule": "Mondays",
                  "bookingUrl": "https://example.com/book",
                  "sourceUrl": "https://example.com/course"
                }
              ],
              "notes": "Available now"
            }
            """));

        var handler = new SearchCoursesCommandHandler(client);
        var result = await handler.HandleAsync(new SearchCoursesCommand(new CourseSearchRequest("pottery", "SW1A 1AA", 10)), CancellationToken.None);

        result.Kind.Should().Be(SearchCoursesOutcomeKind.Success);
        result.Response.Should().NotBeNull();
        result.Response!.Query.HobbyDescription.Should().Be("pottery");
        result.Response.Results.Should().ContainSingle();
        result.Response.Results[0].ProviderName.Should().Be("Studio One");
    }

    [Test]
    public async Task HandleAsync_returns_raw_response_for_malformed_json()
    {
        var client = Substitute.For<ICourseSearchClient>();
        client.SearchAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult("not-json"));

        var handler = new SearchCoursesCommandHandler(client);
        var result = await handler.HandleAsync(new SearchCoursesCommand(new CourseSearchRequest("pottery", "SW1A 1AA", 10)), CancellationToken.None);

        result.Kind.Should().Be(SearchCoursesOutcomeKind.RawResponse);
        result.RawResponse.Should().NotBeNull();
        result.RawResponse!.RawResponse.Should().Be("not-json");
    }
}
