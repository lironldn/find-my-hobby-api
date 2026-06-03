using FluentAssertions;
using FindMyHobbyApi.Domain;
using NSubstitute;

namespace FindMyHobbyApi.Domain.UnitTests.ServiceTests;

public sealed class FindMyHobbyApiServiceTests
{
    [Test]
    public async Task GetHobbyAsync_delegates_to_query_handler()
    {
        var queryHandler = Substitute.For<IGetHobbyQueryHandler>();
        queryHandler.HandleAsync(Arg.Any<GetHobbyQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new[] { new Hobby("Cooking") }));

        var commandHandler = Substitute.For<ISearchCoursesCommandHandler>();
        var service = new FindMyHobbyApiService(queryHandler, commandHandler);

        var result = await service.GetHobbyAsync(CancellationToken.None);

        result.Should().ContainSingle(hobby => hobby.Name == "Cooking");
        await queryHandler.Received(1).HandleAsync(Arg.Any<GetHobbyQuery>(), CancellationToken.None);
        await commandHandler.DidNotReceiveWithAnyArgs().HandleAsync(default!, default);
    }

    [Test]
    public async Task SearchCoursesAsync_delegates_to_command_handler()
    {
        var queryHandler = Substitute.For<IGetHobbyQueryHandler>();
        var commandHandler = Substitute.For<ISearchCoursesCommandHandler>();
        commandHandler.HandleAsync(Arg.Any<SearchCoursesCommand>(), Arg.Any<CancellationToken>())
            .Returns(SearchCoursesOutcome.ValidationError("UK postcode is required."));

        var service = new FindMyHobbyApiService(queryHandler, commandHandler);
        var request = new CourseSearchRequest("pottery", "SW1A 1AA", 10);

        var result = await service.SearchCoursesAsync(request, CancellationToken.None);

        result.Kind.Should().Be(SearchCoursesOutcomeKind.ValidationError);
        await commandHandler.Received(1).HandleAsync(Arg.Any<SearchCoursesCommand>(), CancellationToken.None);
        await queryHandler.DidNotReceiveWithAnyArgs().HandleAsync(default!, default);
    }
}
