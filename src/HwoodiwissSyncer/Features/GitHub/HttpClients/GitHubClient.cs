﻿using System.Net;
using System.Net.Http.Headers;
using HwoodiwissSyncer.Features.GitHub.Services;
using Microsoft.Extensions.Caching.Memory;

namespace HwoodiwissSyncer.Features.GitHub.HttpClients;

public sealed partial class GitHubClient(HttpClient httpClient, IGitHubAppAuthProvider authProvider, IMemoryCache cache, ILogger<GitHubClient> logger) : IGitHubClient
{
    public async Task<Result<Unit>> CreatePullRequestReview(string repoOwner, string repoName, int pullRequestNumber, int installationId, SubmitReviewRequest reviewRequest)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, $"/repos/{repoOwner}/{repoName}/pulls/{pullRequestNumber}/reviews");
        request.Headers.Accept.Add(new("application/vnd.github+json"));
        request.Headers.Add("X-GitHub-Api-Version", "2022-11-28");

        var installationToken = await GetInstallationToken(
            installationId,
            new Dictionary<InstallationScope, InstallationOperation>
            {
                [InstallationScope.PullRequests] = InstallationOperation.Write
            },
            [repoName]);

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", installationToken);
        request.Content = JsonContent.Create(reviewRequest, GitHubClientJsonSerializerContext.Default.SubmitReviewRequest);
        using var response = await httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            Log.FailedToApprovePullRequest(logger, pullRequestNumber, repoOwner, repoName, installationId, (int)response.StatusCode);
            return new Problem.Reason("Failed to approve pull request");
        }

        return Unit.Instance;
    }

    private async Task<string> GetInstallationToken(int installationId, Dictionary<InstallationScope, InstallationOperation> permissions, string[]? repositories)
    {
        return await cache.GetOrCreateAsync<string>(CreateCacheKey(installationId, permissions, repositories), async (entry) =>
        {
            var tokenResult = await RequestInstallationAccessToken(installationId, permissions, repositories);

            if (tokenResult is not Result<InstallationTokenResponse>.Success { Value: {} tokenResponse })
            {
                return entry.Value as string ?? string.Empty;
            }

            entry.AbsoluteExpiration = tokenResponse.ExpiresAt - TimeSpan.FromMinutes(5);

            return tokenResponse.Token;
        }) ?? string.Empty;
    }

    private async Task<Result<InstallationTokenResponse>> RequestInstallationAccessToken(int installationId, Dictionary<InstallationScope, InstallationOperation> permissions, string[]? repositories)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, $"/app/installations/{installationId}/access_tokens");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authProvider.GetGithubJwt());
            request.Headers.Accept.Add(new("application/vnd.github+json"));
            request.Headers.Add("X-GitHub-Api-Version", "2022-11-28");
            request.Content = JsonContent.Create(new InstallationTokenRequest
            {
                Repositories = repositories,
                Permissions = permissions,
            }, GitHubClientJsonSerializerContext.Default.InstallationTokenRequest);

            Log.RefreshingInstallationToken(logger, installationId);
            using var response = await httpClient.SendAsync(request);

            if (response.StatusCode != HttpStatusCode.Created)
            {
                Log.InstallationsTokenRequestFailed(logger, installationId, (int)response.StatusCode, await response.Content.ReadAsStringAsync());
                return new Problem.Reason("Failed to create installation token.");
            }

            var result = await response.Content.ReadFromJsonAsync(GitHubClientJsonSerializerContext.Default.InstallationTokenResponse);

            if (result is null)
            {
                return new Problem.Reason("Installation token request succeeded but the response was null or could not be deserialized.");
            }

            return result;
        }
        catch (Exception ex)
        {
            Log.InstallationsTokenRequestFailedExceptional(logger, installationId, ex);
            return new Problem.Exceptional(ex);
        }
    }

    private static string CreateCacheKey(int installationId, Dictionary<InstallationScope, InstallationOperation> permissions, string[]? repositories)
    {
        var permissionsString = string.Join('_', permissions.Select(s => $"{s.Key}:{s.Value}"));
        var repositoriesString = repositories is not null ? $"_for_{string.Join('_', repositories)}" : "";
        return $"installation_token_{installationId}_with_{permissionsString}{repositoriesString}";
    }

    private static partial class Log
    {
        [LoggerMessage(LogLevel.Information, "Requesting new Installation access token for {InstallationId}")]
        public static partial void RefreshingInstallationToken(ILogger logger, long installationId);

        [LoggerMessage(LogLevel.Error, "Installation token request for {InstallationId} failed with Status {StatusCode} and Response {ResponseContent}")]
        public static partial void InstallationsTokenRequestFailed(ILogger logger, long installationId, int statusCode, string responseContent);

        [LoggerMessage(LogLevel.Error, "Installation token request for {InstallationId} failed")]
        public static partial void InstallationsTokenRequestFailedExceptional(ILogger logger, long installationId, Exception ex);

        [LoggerMessage(LogLevel.Error, "Failed to approve pull request #{PullRequest} in {RepoOrg}/{RepoName} for {InstallationId} with Status {StatusCode}")]
        public static partial void FailedToApprovePullRequest(ILogger logger, int pullRequest, string repoOrg, string repoName, int installationId, int statusCode);
    }
}
