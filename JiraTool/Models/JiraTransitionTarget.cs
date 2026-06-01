using System.Text.Json.Serialization;

namespace JiraTool.Models;

public sealed record JiraTransitionTarget
{
    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;
}