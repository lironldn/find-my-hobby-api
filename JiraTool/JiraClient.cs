using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using JiraTool.Exceptions;
using JiraTool.Models;

namespace JiraTool;

public sealed class JiraClient : IDisposable, IJiraClient
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
        var response = await _httpClient.GetAsync($"/ex/jira/{Uri.EscapeDataString(_cloudId)}/rest/api/3/issue/{Uri.EscapeDataString(issueKey)}?fields=summary,status,labels,description", cancellationToken);
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