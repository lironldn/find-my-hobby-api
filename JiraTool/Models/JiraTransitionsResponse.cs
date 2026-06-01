using System.Text.Json.Serialization;

namespace JiraTool.Models;

public sealed record JiraTransitionsResponse
{
    [JsonPropertyName("transitions")]
    public JiraTransition[] Transitions { get; init; } = Array.Empty<JiraTransition>();
}