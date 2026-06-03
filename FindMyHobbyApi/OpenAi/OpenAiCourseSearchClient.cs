using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using FindMyHobbyApi.Domain;

namespace FindMyHobbyApi.OpenAi;

public sealed class OpenAiCourseSearchClient : ICourseSearchClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public OpenAiCourseSearchClient(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task<string> SearchAsync(string prompt, CancellationToken cancellationToken)
    {
        var openAiApiKey = _configuration["OPENAI_API_KEY"];
        if (string.IsNullOrWhiteSpace(openAiApiKey))
        {
            throw new InvalidOperationException("OpenAI API key is not configured.");
        }

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", openAiApiKey);

        var openAiRequest = new OpenAiResponsesRequest(
            Model: "gpt-4.1",
            Tools:
            [
                new OpenAiTool("web_search_preview")
            ],
            Input: prompt);

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "v1/responses")
        {
            Content = new StringContent(
                JsonSerializer.Serialize(openAiRequest, JsonOptions),
                Encoding.UTF8,
                "application/json")
        };

        using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(responseBody);
        }

        var openAiResponse = JsonSerializer.Deserialize<OpenAiResponsesResponse>(responseBody, JsonOptions);
        return openAiResponse?.OutputText ?? string.Empty;
    }
}
