using PrReviewBuddy.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Allow our frontend (running on localhost:5173) to call this backend.
// Without this, the browser blocks the request for security reasons (this is called CORS).
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddHttpClient<GitHubPrService>();
builder.Services.AddHttpClient<AzureDevOpsPrService>();

var app = builder.Build();

app.UseCors("AllowFrontend");

// This is our very first API endpoint. It just proves the backend is alive.
app.MapGet("/api/hello", () =>
{
    return Results.Ok(new
    {
        message = "PR Review Buddy backend is alive.",
        timestampUtc = DateTime.UtcNow
    });
});

// This is the "Unified PR Queue" — it fetches from GitHub AND Azure DevOps,
// and combines them into one list. If one source fails (e.g. bad token),
// we still show the other source rather than failing the whole request.
app.MapGet("/api/prs", async (GitHubPrService github, AzureDevOpsPrService azureDevOps) =>
{
    var allPrs = new List<PrReviewBuddy.Api.Models.PullRequestSummary>();
    var warnings = new List<string>();

    try
    {
        allPrs.AddRange(await github.GetOpenPullRequestsAsync());
    }
    catch (Exception ex)
    {
        warnings.Add($"GitHub: {ex.Message}");
    }

    try
    {
        allPrs.AddRange(await azureDevOps.GetActivePullRequestsAsync());
    }
    catch (Exception ex)
    {
        warnings.Add($"Azure DevOps: {ex.Message}");
    }

    var sorted = allPrs.OrderByDescending(pr => pr.CreatedAt).ToList();

    return Results.Ok(new { prs = sorted, warnings });
});

app.Run();
