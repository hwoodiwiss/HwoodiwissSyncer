using System.Diagnostics;
using System.Text.RegularExpressions;
using HwoodiwissSyncer.Features.GitHub.Commands;
using HwoodiwissSyncer.Features.GitHub.Configuration;
using HwoodiwissSyncer.Features.GitHub.Events;
using HwoodiwissSyncer.Features.Kubernetes.Services;
using Microsoft.Extensions.Options;

namespace HwoodiwissSyncer.Features.GitHub.Handlers;

public sealed partial class PackagePublishedHandler(
    IKubernetesService kubernetesService,
    IOptions<DeploymentConfiguration> deploymentOptions,
    ILogger<PackagePublishedHandler> logger,
    IMapper<RegistryPackage.Published, UpdateDeploymentImageCommand> mapper,
    ActivitySource activitySource)
    : GithubWebhookRequestHandler<RegistryPackage.Published, UpdateDeploymentImageCommand>(logger, mapper, activitySource)
{
    private readonly DeploymentConfiguration _deploymentConfiguration = deploymentOptions.Value;
    
    protected override async ValueTask<object?> HandleGithubEventAsync(UpdateDeploymentImageCommand request)
    {
        foreach (var ( deploymentName, deploymentConfig ) in _deploymentConfiguration.Deployments)
        {
            if (!deploymentConfig.Image!.Equals(request.ContainerPath))
            {
                continue;
            }
            var supportedRegexes = deploymentConfig.LabelPatterns.Select(s => new Regex(s));
            if (supportedRegexes.Any(s => s.IsMatch(request.ContainerLabel)))
            {
                await kubernetesService.UpdateDeploymentImage(deploymentConfig.Namespace, deploymentName, deploymentConfig.Image, request.ContainerLabel);
            }
        }
        
        return Unit.Instance;
    }
}
