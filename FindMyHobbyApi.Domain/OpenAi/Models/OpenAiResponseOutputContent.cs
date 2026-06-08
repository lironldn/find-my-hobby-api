using System.Text.Json.Serialization;

namespace FindMyHobbyApi.Domain.OpenAi.Models;

record OpenAiResponseOutputContent
{
    [JsonPropertyName("type")]
    public string? Type { get; init; }

    [JsonPropertyName("text")]
    public string? Text { get; init; }
}