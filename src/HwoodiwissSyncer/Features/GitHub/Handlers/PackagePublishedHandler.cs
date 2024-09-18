using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using HwoodiwissSyncer.Features.GitHub.Commands;
using HwoodiwissSyncer.Features.GitHub.Configuration;
using HwoodiwissSyncer.Features.GitHub.Events;
using HwoodiwissSyncer.Features.GitHub.Services;
using HwoodiwissSyncer.Features.Kubernetes.Services;
using Microsoft.Extensions.Options;

namespace HwoodiwissSyncer.Features.GitHub.Handlers;

public sealed partial class PackagePublishedHandler(
    IKubernetesService kubernetesService,
    IGitHubService gitHubService,
    IOptions<DeploymentConfiguration> deploymentOptions,
    ILogger<PackagePublishedHandler> logger,
    IMapper<RegistryPackage.Published, UpdateDeploymentImageCommand> mapper,
    ActivitySource activitySource)
    : GithubWebhookRequestHandler<RegistryPackage.Published, UpdateDeploymentImageCommand>(logger, mapper, activitySource)
{
    private readonly DeploymentConfiguration _deploymentConfiguration = deploymentOptions.Value;

    protected override async ValueTask<object?> HandleGithubEventAsync(UpdateDeploymentImageCommand request)
    {
        foreach (var (deploymentName, deploymentConfig) in _deploymentConfiguration.Deployments)
        {
            if (!deploymentConfig.Image!.Equals(request.ContainerPath))
            {
                continue;
            }
            var supportedRegexes = deploymentConfig.LabelPatterns.Select(s => new Regex(s));
            if (supportedRegexes.Any(s => s.IsMatch(request.ContainerLabel)))
            {
                return await kubernetesService.UpdateDeploymentImage(deploymentConfig.Namespace, deploymentName,
                        deploymentConfig.Image, request.ContainerLabel) switch
                    {
                        Result<Unit>.Success => await PostPullRequstMessage(request),
                        Result<Unit>.Failure => Unit.Instance,
                        _ => throw new UnreachableException()
                    };
            }
        }

        return Unit.Instance;
    }

    private async ValueTask<object?> PostPullRequstMessage(UpdateDeploymentImageCommand command)
    {
        if (PullRequestRegex.Match(command.ContainerLabel) is {Success: true} match)
        {
            string prNumberString = match.Groups["prnumber"].Value;
            int prNumber = int.Parse(prNumberString);

            await gitHubService.CreateIssueComment(
                command.RepoOwner,
                command.RepoName,
                prNumber,
                command.InstallationId,
                $"## Deployed\nUpdated deployment to {command.ContainerPath}:{command.ContainerLabel}"
            );
        }

        return Unit.Instance;
    }

    [GeneratedRegex("pr-(?<prnumber>[0-9]+)-arm64")]
    private partial Regex PullRequestRegex { get; }
}
