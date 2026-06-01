using System.Text.Json;
using System.Text.Json.Nodes;

namespace JiraTool;

internal static class JiraDescriptionBuilder
{
    public static JsonNode AppendText(JsonElement description, string text)
    {
        if (description.ValueKind is JsonValueKind.Object)
        {
            var node = JsonNode.Parse(description.GetRawText());
            if (node is JsonObject document)
            {
                if (document["content"] is JsonArray content)
                {
                    content.Add(CreateParagraph(text));
                    return document;
                }
            }
        }

        return CreateDocument(text);
    }

    private static JsonObject CreateDocument(string text)
        => new()
        {
            ["type"] = "doc",
            ["version"] = 1,
            ["content"] = new JsonArray(CreateParagraph(text))
        };

    private static JsonObject CreateParagraph(string text)
        => new()
        {
            ["type"] = "paragraph",
            ["content"] = new JsonArray
            {
                new JsonObject
                {
                    ["type"] = "text",
                    ["text"] = text
                }
            }
        };
}