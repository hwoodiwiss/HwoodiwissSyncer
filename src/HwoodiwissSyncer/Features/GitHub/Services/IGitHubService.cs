namespace HwoodiwissSyncer.Features.GitHub.Services;

public interface IGitHubService
{
    Task CreateIssueComment(string repoOwner, string repoName, int pullRequestNumber, int installationId, string commentBody);
}
