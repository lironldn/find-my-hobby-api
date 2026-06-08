using System.Text.Json.Serialization;

namespace FindMyHobbyApi.Domain.OpenAi.Models;

record OpenAiResponseOutputItem
{
    [JsonPropertyName("type")]
    public string? Type { get; init; }

    [JsonPropertyName("content")]
    public OpenAiResponseOutputContent[]? Content { get; init; }
}