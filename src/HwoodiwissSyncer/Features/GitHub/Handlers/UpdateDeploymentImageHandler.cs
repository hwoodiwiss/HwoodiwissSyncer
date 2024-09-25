using System.Diagnostics;
using System.Text.RegularExpressions;
using HwoodiwissSyncer.Features.GitHub.Commands;
using HwoodiwissSyncer.Features.GitHub.Configuration;
using HwoodiwissSyncer.Features.GitHub.Services;
using HwoodiwissSyncer.Features.Kubernetes.Services;
using HwoodiwissSyncer.Handlers;
using Microsoft.Extensions.Options;

namespace HwoodiwissSyncer.Features.GitHub.Handlers;

public sealed partial class UpdateDeploymentImageHandler(
    IKubernetesService kubernetesService,
    IGitHubService gitHubService,
    IOptions<DeploymentConfiguration> deploymentOptions)
    : IRequestHandler<UpdateDeploymentImageCommand>
{
    private readonly DeploymentConfiguration _deploymentConfiguration = deploymentOptions.Value;

    public async ValueTask<object?> HandleAsync(UpdateDeploymentImageCommand request)
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
