using System.Text.Json;

namespace JiraTool;

public static class JiraIssueRenderer
{
    public static string ExtractText(JsonElement description)
    {
        if (description.ValueKind is not JsonValueKind.Object)
        {
            return string.Empty;
        }

        return ExtractTextFromNode(description).Trim();
    }

    private static string ExtractTextFromNode(JsonElement node)
    {
        if (node.ValueKind is JsonValueKind.String)
        {
            return node.GetString() ?? string.Empty;
        }

        if (node.ValueKind is JsonValueKind.Array)
        {
            var parts = node.EnumerateArray()
                .Select(ExtractTextFromNode)
                .Where(text => !string.IsNullOrWhiteSpace(text))
                .ToArray();

            return string.Join(Environment.NewLine, parts);
        }

        if (node.ValueKind is JsonValueKind.Object)
        {
            if (node.TryGetProperty("text", out var text))
            {
                return text.GetString() ?? string.Empty;
            }

            if (node.TryGetProperty("content", out var content))
            {
                return ExtractTextFromNode(content);
            }
        }

        return string.Empty;
    }
}