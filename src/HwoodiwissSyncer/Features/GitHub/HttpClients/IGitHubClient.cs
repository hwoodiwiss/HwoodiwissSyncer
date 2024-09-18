namespace HwoodiwissSyncer.Features.GitHub.HttpClients;

public interface IGitHubClient
{
    Task<Result<Unit>> CreateIssueComment(string repoOwner, string repoName, int issueNumber, int installationId, string commentBody);
}
