using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Reqnroll;

namespace FindMyHobbyApi.AcceptanceTests.StepDefinitions;

[Binding]
public sealed class GetHobbyStepDefinitions : IDisposable
{
    private readonly HttpClient _httpClient;
    private HttpResponseMessage? _response;
    private HobbyResponse[]? _hobbies;

    public GetHobbyStepDefinitions()
    {
        var baseUrl = Environment.GetEnvironmentVariable("FIND_MY_HOBBY_API_BASE_URL")
                      ?? "http://localhost:5001";

        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(baseUrl)
        };
    }

    [Given("the Find My Hobby API is available")]
    public async Task GivenTheFindMyHobbyApiIsAvailable()
    {
        using var response = await _httpClient.GetAsync("/health");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [When("I request hobby suggestions")]
    public async Task WhenIRequestHobbySuggestions()
    {
        _response?.Dispose();
        _response = await _httpClient.GetAsync("/hobby");
    }

    [Then("the response status code should be {int}")]
    public void ThenTheResponseStatusCodeShouldBe(int expectedStatusCode)
    {
        _response.Should().NotBeNull();
        ((int)_response!.StatusCode).Should().Be(expectedStatusCode);
    }

    [Then("the response should contain {int} hobbies")]
    public async Task ThenTheResponseShouldContainHobbies(int expectedCount)
    {
        _response.Should().NotBeNull();

        _hobbies = await _response!.Content.ReadFromJsonAsync<HobbyResponse[]>();

        _hobbies.Should().NotBeNull();
        _hobbies.Should().HaveCount(expectedCount);
    }

    [Then("each hobby should have a name")]
    public void ThenEachHobbyShouldHaveAName()
    {
        _hobbies.Should().NotBeNull();
        _hobbies.Should().OnlyContain(hobby => !string.IsNullOrWhiteSpace(hobby.Name));
    }

    public void Dispose()
    {
        _response?.Dispose();
        _httpClient.Dispose();
    }

    private sealed record HobbyResponse(string Name);
}