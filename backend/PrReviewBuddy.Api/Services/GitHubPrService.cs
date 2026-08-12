using System.Net.Http.Headers;
using System.Text.Json;
using PrReviewBuddy.Api.Models;

namespace PrReviewBuddy.Api.Services;

public class GitHubPrService
{
    private readonly HttpClient _http;
    private readonly IConfiguration _config;

    public GitHubPrService(HttpClient http, IConfiguration config)
    {
        _http = http;
        _config = config;

        // GitHub requires a User-Agent header on every request, or it rejects us.
        if (!_http.DefaultRequestHeaders.UserAgent.Any())
        {
            _http.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("PrReviewBuddy", "0.1"));
        }
    }

    public async Task<List<PullRequestSummary>> GetOpenPullRequestsAsync()
    {
        var token = _config["GitHub:Token"];
        var owner = _config["GitHub:Owner"];
        var repo = _config["GitHub:Repo"];

        if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(owner) || string.IsNullOrWhiteSpace(repo))
        {
            throw new InvalidOperationException(
                "GitHub configuration is missing. Set GitHub:Token, GitHub:Owner, and GitHub:Repo using 'dotnet user-secrets'.");
        }

        var request = new HttpRequestMessage(HttpMethod.Get, $"https://api.github.com/repos/{owner}/{repo}/pulls?state=open");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));

        var response = await _http.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException(
                $"GitHub API returned {(int)response.StatusCode} {response.StatusCode}. Body: {errorBody}");
        }

        var stream = await response.Content.ReadAsStreamAsync();
        var doc = await JsonDocument.ParseAsync(stream);

        var results = new List<PullRequestSummary>();

        foreach (var pr in doc.RootElement.EnumerateArray())
        {
            results.Add(new PullRequestSummary(
                Number: pr.GetProperty("number").GetInt32(),
                Title: pr.GetProperty("title").GetString() ?? "(untitled)",
                AuthorLogin: pr.GetProperty("user").GetProperty("login").GetString() ?? "unknown",
                HtmlUrl: pr.GetProperty("html_url").GetString() ?? "",
                IsDraft: pr.TryGetProperty("draft", out var draftProp) && draftProp.GetBoolean(),
                CreatedAt: pr.GetProperty("created_at").GetDateTimeOffset(),
                State: pr.GetProperty("state").GetString() ?? "open",
                Source: "GitHub"
            ));
        }

        return results;
    }
}
