using System.Text.Json.Serialization;

namespace FindMyHobbyApi.Domain.OpenAi.Models;

record OpenAiResponse
{
    [JsonPropertyName("output")]
    public OpenAiResponseOutputItem[]? Output { get; init; }

    [JsonIgnore]
    public string? OutputText =>
        Output?
            .SelectMany(item => item.Content ?? Array.Empty<OpenAiResponseOutputContent>())
            .FirstOrDefault(content => content.Type == "output_text")
            ?.Text;
}