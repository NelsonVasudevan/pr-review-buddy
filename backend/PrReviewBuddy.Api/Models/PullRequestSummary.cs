namespace PrReviewBuddy.Api.Models;

// This is what we send back to our own frontend — a simplified, clean shape.
public record PullRequestSummary(
    int Number,
    string Title,
    string AuthorLogin,
    string HtmlUrl,
    bool IsDraft,
    DateTimeOffset CreatedAt,
    string State,
    string Source // "GitHub" or "AzureDevOps"
);
