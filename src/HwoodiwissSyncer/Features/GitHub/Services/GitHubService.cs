using System.Diagnostics;
using HwoodiwissSyncer.Features.GitHub.HttpClients;

namespace HwoodiwissSyncer.Features.GitHub.Services;

public sealed partial class GitHubService(IGitHubClient githubClient, ActivitySource activitySource, ILogger<GitHubService> logger) : IGitHubService
{

    public async Task CreatePullRequestComment(string repoOwner, string repoName, int pullRequestNumber, int installationId, string commentBody)
    {
        using var activity = activitySource.StartActivity();
        activity?.SetTag("pullrequest.number", pullRequestNumber);
        activity?.SetTag("pullrequest.repo", $"{repoOwner}/{repoName}");

        try
        {
            await githubClient.CreatePullRequestReview(repoOwner, repoName, pullRequestNumber, installationId,
                new SubmitReviewRequest
                {
                    Body = commentBody,
                    Event = SubmitReviewEvent.Comment,
                });
        }
        catch (Exception error)
        {
            activity?.SetTag("exception.type", error.GetType().Name);
            Log.FailedToApprovePullRequest(logger, pullRequestNumber, repoOwner, repoName, installationId);
        }
    }

    private static partial class Log
    {
        [LoggerMessage(LogLevel.Error, "Failed to comment on pull request #{PullRequest} in {RepoOrg}/{RepoName} for {InstallationId}")]
        public static partial void FailedToApprovePullRequest(ILogger logger, int pullRequest, string repoOrg, string repoName, int installationId);
    }
}
