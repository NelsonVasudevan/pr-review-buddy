using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using PrReviewBuddy.Api.Models;

namespace PrReviewBuddy.Api.Services;

public class AzureDevOpsPrService
{
    private readonly HttpClient _http;
    private readonly IConfiguration _config;

    public AzureDevOpsPrService(HttpClient http, IConfiguration config)
    {
        _http = http;
        _config = config;
    }

    public async Task<List<PullRequestSummary>> GetActivePullRequestsAsync()
    {
        var token = _config["AzureDevOps:Token"];
        var org = _config["AzureDevOps:Organization"];
        var project = _config["AzureDevOps:Project"];
        var repo = _config["AzureDevOps:Repo"];

        if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(org) ||
            string.IsNullOrWhiteSpace(project) || string.IsNullOrWhiteSpace(repo))
        {
            throw new InvalidOperationException(
                "Azure DevOps configuration is missing. Set AzureDevOps:Token, AzureDevOps:Organization, AzureDevOps:Project, and AzureDevOps:Repo using 'dotnet user-secrets'.");
        }

        // Azure DevOps uses "Basic" auth where the username is blank and the password is the token.
        var basicAuth = Convert.ToBase64String(Encoding.ASCII.GetBytes($":{token}"));

        var url = $"https://dev.azure.com/{org}/{project}/_apis/git/repositories/{repo}/pullrequests" +
                  "?searchCriteria.status=active&api-version=7.1";

        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", basicAuth);

        var response = await _http.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException(
                $"Azure DevOps API returned {(int)response.StatusCode} {response.StatusCode}. Body: {errorBody}");
        }

        var stream = await response.Content.ReadAsStreamAsync();
        var doc = await JsonDocument.ParseAsync(stream);

        var results = new List<PullRequestSummary>();

        foreach (var pr in doc.RootElement.GetProperty("value").EnumerateArray())
        {
            var id = pr.GetProperty("pullRequestId").GetInt32();
            var authorName = pr.GetProperty("createdBy").GetProperty("displayName").GetString() ?? "unknown";
            var webUrl = $"https://dev.azure.com/{org}/{project}/_git/{repo}/pullrequest/{id}";

            results.Add(new PullRequestSummary(
                Number: id,
                Title: pr.GetProperty("title").GetString() ?? "(untitled)",
                AuthorLogin: authorName,
                HtmlUrl: webUrl,
                IsDraft: pr.TryGetProperty("isDraft", out var draftProp) && draftProp.GetBoolean(),
                CreatedAt: pr.GetProperty("creationDate").GetDateTimeOffset(),
                State: pr.GetProperty("status").GetString() ?? "active",
                Source: "AzureDevOps"
            ));
        }

        return results;
    }
}
