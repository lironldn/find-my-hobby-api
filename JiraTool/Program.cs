using JiraTool.Exceptions;

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