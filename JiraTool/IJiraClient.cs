using JiraTool.Models;

namespace JiraTool;

public interface IJiraClient
{
    Task<JiraIssue> GetIssueAsync(string issueKey, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<JiraTransition>> GetTransitionsAsync(string issueKey, CancellationToken cancellationToken = default);
    Task TransitionIssueAsync(string issueKey, string transitionId, CancellationToken cancellationToken = default);
    Task AddLabelAsync(string issueKey, string label, CancellationToken cancellationToken = default);
    Task AppendDescriptionAsync(string issueKey, string text, CancellationToken cancellationToken = default);
}