using JiraTool.Exceptions;

namespace JiraTool;

public sealed class JiraConfig
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

            Environment.SetEnvironmentVariable(key, value);
        }
    }

}