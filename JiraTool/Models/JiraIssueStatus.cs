using System.Text.Json.Serialization;

namespace JiraTool.Models;

public sealed record JiraIssueStatus
{
    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;
}