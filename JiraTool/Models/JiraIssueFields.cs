using System.Text.Json;
using System.Text.Json.Serialization;

namespace JiraTool.Models;

public sealed record JiraIssueFields
{
    [JsonPropertyName("summary")]
    public string Summary { get; init; } = string.Empty;

    [JsonPropertyName("status")]
    public JiraIssueStatus Status { get; init; } = new();

    [JsonPropertyName("labels")]
    public string[] Labels { get; init; } = Array.Empty<string>();

    [JsonPropertyName("description")]
    public JsonElement Description { get; init; }
}