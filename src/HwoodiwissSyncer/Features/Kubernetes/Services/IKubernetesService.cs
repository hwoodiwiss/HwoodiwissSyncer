namespace HwoodiwissSyncer.Features.Kubernetes.Services;

public interface IKubernetesService
{
    Task<Result<Unit>> UpdateDeploymentImage(string nameSpace, string deploymentName, string imagePath, string imageVersion);
}
