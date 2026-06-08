using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using FindMyHobbyApi.Domain;
using FindMyHobbyApi.OpenAi.Models;

namespace FindMyHobbyApi.OpenAi;

public sealed class OpenAiCourseSearchClient(HttpClient httpClient, IConfiguration configuration) : ICourseSearchClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<string> SearchAsync(string prompt, CancellationToken cancellationToken)
    {
        var openAiApiKey = configuration["OPENAI_API_KEY"];
        if (string.IsNullOrWhiteSpace(openAiApiKey))
        {
            throw new InvalidOperationException("OpenAI API key is not configured.");
        }

        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", openAiApiKey);

        var openAiRequest = new OpenAiRequest(
            Model: "gpt-4.1",
            Tools:
            [
                new OpenAiTool("web_search_preview")
            ],
            Input: prompt);

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "v1/responses");
        httpRequest.Content = new StringContent(
            JsonSerializer.Serialize(openAiRequest, JsonOptions),
            Encoding.UTF8,
            "application/json");

        using var response = await httpClient.SendAsync(httpRequest, cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(responseBody);
        }

        var openAiResponse = JsonSerializer.Deserialize<OpenAiResponse>(responseBody, JsonOptions);
        return openAiResponse?.OutputText ?? string.Empty;
    }
}
