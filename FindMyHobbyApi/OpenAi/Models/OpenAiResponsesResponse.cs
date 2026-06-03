using System.Text.Json.Serialization;

record OpenAiResponsesResponse
{
    [JsonPropertyName("output")]
    public OpenAiResponseOutputItem[]? Output { get; init; }

    [JsonIgnore]
    public string? OutputText =>
        Output?
            .SelectMany(item => item.Content ?? Array.Empty<OpenAiResponseOutputContent>())
            .FirstOrDefault(content => content.Type == "output_text")
            ?.Text;
}

record OpenAiResponseOutputItem
{
    [JsonPropertyName("type")]
    public string? Type { get; init; }

    [JsonPropertyName("content")]
    public OpenAiResponseOutputContent[]? Content { get; init; }
}

record OpenAiResponseOutputContent
{
    [JsonPropertyName("type")]
    public string? Type { get; init; }

    [JsonPropertyName("text")]
    public string? Text { get; init; }
}
