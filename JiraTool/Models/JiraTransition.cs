using System.Text.Json.Serialization;

namespace JiraTool.Models;

public sealed record JiraTransition
{
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("to")]
    public JiraTransitionTarget? To { get; init; }
}