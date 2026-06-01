using System.Text.Json.Serialization;

namespace JiraTool.Models;

public sealed record JiraIssue
{
    [JsonPropertyName("key")]
    public string Key { get; init; } = string.Empty;

    [JsonPropertyName("fields")]
    public JiraIssueFields Fields { get; init; } = new();
}