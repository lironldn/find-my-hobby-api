using System.Net;
using FluentAssertions;
using FindMyHobbyApi.Domain;
using FindMyHobbyApi;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using NSubstitute;

namespace FindMyHobbyApi.Domain.UnitTests.ControllerTests;

public sealed class FindMyHobbyApiControllerTests
{
    [Test]
    public async Task GetHobbyAsync_returns_ok_result_from_service()
    {
        var service = Substitute.For<IFindMyHobbyApiService>();
        service.GetHobbyAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new[] { new Hobby("Pottery") }));

        var controller = new FindMyHobbyApiController(service);

        var result = await controller.GetHobbyAsync(CancellationToken.None);

        var ok = result.Should().BeOfType<Ok<Hobby[]>>().Subject;
        ok.Value.Should().ContainSingle(hobby => hobby.Name == "Pottery");
        await service.Received(1).GetHobbyAsync(CancellationToken.None);
    }

    [Test]
    public async Task SearchCoursesAsync_returns_bad_request_for_validation_errors()
    {
        var service = Substitute.For<IFindMyHobbyApiService>();
        service.SearchCoursesAsync(Arg.Any<CourseSearchRequest>(), Arg.Any<CancellationToken>())
            .Returns(SearchCoursesOutcome.ValidationError("Hobby description is required."));

        var controller = new FindMyHobbyApiController(service);

        var result = await controller.SearchCoursesAsync(new CourseSearchRequest("", "SW1A 1AA", 10), CancellationToken.None);

        var badRequest = result.Should().BeOfType<BadRequest<ProblemDetailsResponse>>().Subject;
        badRequest.Value!.Detail.Should().Be("Hobby description is required.");
        await service.Received(1).SearchCoursesAsync(Arg.Any<CourseSearchRequest>(), CancellationToken.None);
    }

    [Test]
    public async Task SearchCoursesAsync_returns_ok_raw_response_for_unparsed_output()
    {
        var service = Substitute.For<IFindMyHobbyApiService>();
        service.SearchCoursesAsync(Arg.Any<CourseSearchRequest>(), Arg.Any<CancellationToken>())
            .Returns(SearchCoursesOutcome.Raw(new CourseSearchRawResponse("{\"unexpected\":true}")));

        var controller = new FindMyHobbyApiController(service);

        var result = await controller.SearchCoursesAsync(new CourseSearchRequest("pottery", "SW1A 1AA", 10), CancellationToken.None);

        var ok = result.Should().BeOfType<Ok<CourseSearchRawResponse>>().Subject;
        ok.Value!.RawResponse.Should().Be("{\"unexpected\":true}");
    }

    [Test]
    public async Task SearchCoursesAsync_returns_problem_for_upstream_failures()
    {
        var service = Substitute.For<IFindMyHobbyApiService>();
        service.SearchCoursesAsync(Arg.Any<CourseSearchRequest>(), Arg.Any<CancellationToken>())
            .Returns(SearchCoursesOutcome.UpstreamFailure("OpenAI returned an empty response."));

        var controller = new FindMyHobbyApiController(service);

        var result = await controller.SearchCoursesAsync(new CourseSearchRequest("pottery", "SW1A 1AA", 10), CancellationToken.None);

        result.Should().BeAssignableTo<IStatusCodeHttpResult>()
            .Which.StatusCode.Should().Be((int)HttpStatusCode.BadGateway);
    }
}
