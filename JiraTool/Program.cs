using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace JiraTool;

internal static class Program
{
    private const string ReadyStatusName = "READY";

    public static async Task<int> Main(string[] args)
    {
        if (args.Length == 0 || IsHelp(args[0]))
        {
            PrintUsage();
            return args.Length == 0 ? 1 : 0;
        }

        try
        {
            var config = JiraConfig.FromEnvironment();
            using var client = new JiraClient(config);

            return args[0].ToLowerInvariant() switch
            {
                "show" => await ShowIssueAsync(client, args),
                "ready" => await MoveToReadyAsync(client, args),
                "add-label" => await AddLabelAsync(client, args),
                "append-description" => await AppendDescriptionAsync(client, args),
                _ => Fail($"Unknown command '{args[0]}'.")
            };
        }
        catch (JiraConfigurationException ex)
        {
            return Fail(ex.Message);
        }
        catch (JiraApiException ex)
        {
            return Fail(ex.Message);
        }
    }

    private static async Task<int> ShowIssueAsync(JiraClient client, string[] args)
    {
        var issueKey = GetRequiredArgument(args, 1, "issue key");
        var issue = await client.GetIssueAsync(issueKey);

        Console.WriteLine($"{issue.Key} | {issue.Fields.Status.Name}");
        Console.WriteLine(issue.Fields.Summary);

        if (issue.Fields.Labels.Length > 0)
        {
            Console.WriteLine($"Labels: {string.Join(", ", issue.Fields.Labels)}");
        }

        var description = JiraIssueRenderer.ExtractText(issue.Fields.Description);
        if (!string.IsNullOrWhiteSpace(description))
        {
            Console.WriteLine("Description:");
            Console.WriteLine(description);
        }

        return 0;
    }

    private static async Task<int> MoveToReadyAsync(JiraClient client, string[] args)
    {
        var issueKey = GetRequiredArgument(args, 1, "issue key");
        var issue = await client.GetIssueAsync(issueKey);

        if (string.Equals(issue.Fields.Status.Name, ReadyStatusName, StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine($"{issue.Key} is already in {ReadyStatusName}.");
            return 0;
        }

        var transitions = await client.GetTransitionsAsync(issueKey);
        var readyTransition = transitions.FirstOrDefault(transition =>
            string.Equals(transition.Name, ReadyStatusName, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(transition.To?.Name, ReadyStatusName, StringComparison.OrdinalIgnoreCase) ||
            transition.Name.Contains("ready", StringComparison.OrdinalIgnoreCase) ||
            transition.To?.Name.Contains("ready", StringComparison.OrdinalIgnoreCase) == true);

        if (readyTransition is null)
        {
            Console.Error.WriteLine($"No transition to {ReadyStatusName} is available for {issueKey}.");
            Console.Error.WriteLine("Available transitions:");
            foreach (var transition in transitions)
            {
                Console.Error.WriteLine($"- {transition.Name} ({transition.Id}) -> {transition.To?.Name ?? "unknown"}");
            }

            return 1;
        }

        await client.TransitionIssueAsync(issueKey, readyTransition.Id);
        Console.WriteLine($"Moved {issueKey} to {ReadyStatusName} using transition '{readyTransition.Name}'.");
        return 0;
    }

    private static async Task<int> AddLabelAsync(JiraClient client, string[] args)
    {
        var issueKey = GetRequiredArgument(args, 1, "issue key");
        var label = GetRequiredArgument(args, 2, "label");

        await client.AddLabelAsync(issueKey, label);
        Console.WriteLine($"Added label '{label}' to {issueKey}.");
        return 0;
    }

    private static async Task<int> AppendDescriptionAsync(JiraClient client, string[] args)
    {
        var issueKey = GetRequiredArgument(args, 1, "issue key");
        var text = GetRequiredRemainingText(args, 2, "text to append");

        await client.AppendDescriptionAsync(issueKey, text);
        Console.WriteLine($"Appended description text to {issueKey}.");
        return 0;
    }

    private static string GetRequiredArgument(string[] args, int index, string name)
    {
        if (args.Length <= index || string.IsNullOrWhiteSpace(args[index]))
        {
            throw new JiraConfigurationException($"Missing {name}.");
        }

        return args[index];
    }

    private static string GetRequiredRemainingText(string[] args, int startIndex, string name)
    {
        if (args.Length <= startIndex)
        {
            throw new JiraConfigurationException($"Missing {name}.");
        }

        var text = string.Join(' ', args.Skip(startIndex)).Trim();
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new JiraConfigurationException($"Missing {name}.");
        }

        return text;
    }

    private static bool IsHelp(string arg)
        => arg is "-h" or "--help" or "help";

    private static int Fail(string message)
    {
        Console.Error.WriteLine(message);
        Console.Error.WriteLine();
        PrintUsage();
        return 1;
    }

    private static void PrintUsage()
    {
        Console.WriteLine("""
Usage:
  dotnet run --project JiraTool show <issueKey>
  dotnet run --project JiraTool ready <issueKey>
  dotnet run --project JiraTool add-label <issueKey> <label>
  dotnet run --project JiraTool append-description <issueKey> <text>

Environment:
  JIRA_BASE_URL     e.g. https://api.atlassian.com
  JIRA_CLOUD_ID     Jira cloud id
  JIRA_API_TOKEN    Atlassian API token
""");
    }
}

internal sealed class JiraClient : IDisposable
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _httpClient;
    private readonly string _cloudId;

    public JiraClient(JiraConfig config)
    {
        _cloudId = config.CloudId;
        _httpClient = new HttpClient
        {
            BaseAddress = config.BaseUrl
        };

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", config.ApiToken);
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    public void Dispose() => _httpClient.Dispose();

    public async Task<JiraIssue> GetIssueAsync(string issueKey, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync($"/ex/jira/{Uri.EscapeDataString(_cloudId)}/rest/api/3/issue/{Uri.EscapeDataString(issueKey)}?fields=summary,status,labels", cancellationToken);
        return await ReadJsonAsync<JiraIssue>(response, cancellationToken);
    }

    public async Task<IReadOnlyList<JiraTransition>> GetTransitionsAsync(string issueKey, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync($"/ex/jira/{Uri.EscapeDataString(_cloudId)}/rest/api/3/issue/{Uri.EscapeDataString(issueKey)}/transitions", cancellationToken);
        var payload = await ReadJsonAsync<JiraTransitionsResponse>(response, cancellationToken);
        return payload.Transitions;
    }

    public async Task TransitionIssueAsync(string issueKey, string transitionId, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsync(
            $"/ex/jira/{Uri.EscapeDataString(_cloudId)}/rest/api/3/issue/{Uri.EscapeDataString(issueKey)}/transitions",
            JsonContent.Create(new { transition = new { id = transitionId } }),
            cancellationToken);

        await EnsureSuccessAsync(response, cancellationToken);
    }

    public async Task AddLabelAsync(string issueKey, string label, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PutAsync(
            $"/ex/jira/{Uri.EscapeDataString(_cloudId)}/rest/api/3/issue/{Uri.EscapeDataString(issueKey)}",
            JsonContent.Create(new
            {
                update = new
                {
                    labels = new[]
                    {
                        new { add = label }
                    }
                }
            }),
            cancellationToken);

        await EnsureSuccessAsync(response, cancellationToken);
    }

    public async Task AppendDescriptionAsync(string issueKey, string text, CancellationToken cancellationToken = default)
    {
        var issue = await GetIssueAsync(issueKey, cancellationToken);
        var descriptionDocument = JiraDescriptionBuilder.AppendText(issue.Fields.Description, text);

        var response = await _httpClient.PutAsync(
            $"/ex/jira/{Uri.EscapeDataString(_cloudId)}/rest/api/3/issue/{Uri.EscapeDataString(issueKey)}",
            JsonContent.Create(new
            {
                fields = new
                {
                    description = descriptionDocument
                }
            }),
            cancellationToken);

        await EnsureSuccessAsync(response, cancellationToken);
    }

    private static async Task<T> ReadJsonAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new JiraApiException($"Jira request failed with {(int)response.StatusCode} {response.ReasonPhrase}: {body}");
        }

        var result = await response.Content.ReadFromJsonAsync<T>(JsonOptions, cancellationToken);
        return result ?? throw new JiraApiException("Jira returned an empty response.");
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        throw new JiraApiException($"Jira request failed with {(int)response.StatusCode} {response.ReasonPhrase}: {body}");
    }
}

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

internal static class JiraIssueRenderer
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

internal sealed class JiraConfig
{
    public required Uri BaseUrl { get; init; }
    public required string CloudId { get; init; }
    public required string ApiToken { get; init; }

    public static JiraConfig FromEnvironment()
    {
        LoadDotEnvIfPresent();

        var baseUrl = GetOptionalEnvironmentVariable("JIRA_BASE_URL") ?? "https://api.atlassian.com";
        var cloudId = GetRequiredEnvironmentVariable("JIRA_CLOUD_ID");
        var apiToken = GetRequiredEnvironmentVariable("JIRA_API_TOKEN");

        if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var parsedBaseUrl))
        {
            throw new JiraConfigurationException($"JIRA_BASE_URL is not a valid absolute URL: '{baseUrl}'.");
        }

        return new JiraConfig
        {
            BaseUrl = parsedBaseUrl,
            CloudId = cloudId,
            ApiToken = apiToken
        };
    }

    private static string? GetOptionalEnvironmentVariable(string name)
    {
        var value = Environment.GetEnvironmentVariable(name);
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }

    private static string GetRequiredEnvironmentVariable(string name)
    {
        var value = Environment.GetEnvironmentVariable(name);
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new JiraConfigurationException($"Environment variable '{name}' is required.");
        }

        return value;
    }

    private static void LoadDotEnvIfPresent()
    {
        var directory = Directory.GetCurrentDirectory();

        while (!string.IsNullOrWhiteSpace(directory))
        {
            var path = Path.Combine(directory, ".env");
            if (File.Exists(path))
            {
                LoadDotEnvFile(path);
                return;
            }

            directory = Directory.GetParent(directory)?.FullName;
        }
    }

    private static void LoadDotEnvFile(string path)
    {
        foreach (var rawLine in File.ReadAllLines(path))
        {
            var line = rawLine.Trim();
            if (line.Length == 0 || line.StartsWith('#'))
            {
                continue;
            }

            var equalsIndex = line.IndexOf('=');
            if (equalsIndex <= 0)
            {
                continue;
            }

            var key = line[..equalsIndex].Trim();
            var value = line[(equalsIndex + 1)..].Trim();

            if (value.Length >= 2 &&
                ((value.StartsWith('"') && value.EndsWith('"')) ||
                 (value.StartsWith('\'') && value.EndsWith('\''))))
            {
                value = value[1..^1];
            }

            if (string.IsNullOrWhiteSpace(key))
            {
                continue;
            }

            if (Environment.GetEnvironmentVariable(key) is null)
            {
                Environment.SetEnvironmentVariable(key, value);
            }
        }
    }

}

internal sealed class JiraConfigurationException(string message) : Exception(message);

internal sealed class JiraApiException(string message) : Exception(message);

internal sealed record JiraIssue
{
    [JsonPropertyName("key")]
    public string Key { get; init; } = string.Empty;

    [JsonPropertyName("fields")]
    public JiraIssueFields Fields { get; init; } = new();
}

internal sealed record JiraIssueFields
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

internal sealed record JiraIssueStatus
{
    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;
}

internal sealed record JiraTransitionsResponse
{
    [JsonPropertyName("transitions")]
    public JiraTransition[] Transitions { get; init; } = Array.Empty<JiraTransition>();
}

internal sealed record JiraTransition
{
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("to")]
    public JiraTransitionTarget? To { get; init; }
}

internal sealed record JiraTransitionTarget
{
    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;
}
