namespace HwoodiwissSyncer.Features.GitHub.Services;

public interface IGitHubService
{
    Task CreatePullRequestComment(string repoOwner, string repoName, int pullRequestNumber, int installationId, string commentBody);
}
