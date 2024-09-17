namespace HwoodiwissSyncer.Features.GitHub.Commands;

public sealed record UpdateDeploymentImageCommand(
    string ContainerLabel,
    string ContainerPath
);
