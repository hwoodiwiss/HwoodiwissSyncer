namespace HwoodiwissSyncer.Features.GitHub.Configuration;

public sealed class DeploymentConfiguration
{
    public Dictionary<string, ContainerConfiguration> Deployments { get; init; } = [];
}
