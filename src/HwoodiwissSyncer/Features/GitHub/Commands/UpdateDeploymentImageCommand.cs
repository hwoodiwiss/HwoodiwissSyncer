namespace HwoodiwissSyncer.Features.GitHub.Commands;

public sealed record UpdateDeploymentImageCommand(
    string ContainerLabel,
    string ContainerPath,
    int InstallationId,
    string RepoName,
    string RepoOwner
);
