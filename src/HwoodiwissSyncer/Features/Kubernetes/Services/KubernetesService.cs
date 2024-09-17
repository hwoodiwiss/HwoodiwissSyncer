using k8s;
using k8s.Models;

namespace HwoodiwissSyncer.Features.Kubernetes.Services;

public sealed partial class KubernetesService(IKubernetes kubeClient, ILogger<KubernetesService> logger) : IKubernetesService
{
    public async Task<Result<Unit>> UpdateDeploymentImage(string nameSpace, string deploymentName, string imagePath, string imageVersion)
    {
        try
        {

            var deployments = await kubeClient.AppsV1.ListNamespacedDeploymentAsync(nameSpace);

            var matchingDeployment = deployments.Items.FirstOrDefault(w => w.Metadata.Name.Equals(deploymentName));

            var containerSpec = matchingDeployment?.Spec.Template.Spec.Containers.FirstOrDefault(w =>
                w.Image.StartsWith(imagePath, StringComparison.OrdinalIgnoreCase));

            if (containerSpec is not null)
            {
                await kubeClient.AppsV1.PatchNamespacedDeploymentAsync(
                    new V1Patch(CreateDeploymentImagePatchConfig(deploymentName, imagePath, imageVersion),
                        V1Patch.PatchType.MergePatch), deploymentName,
                    nameSpace);
            }

        }
        catch (Exception ex)
        {
            Log.DeploymentUpdateFailed(logger, ex);
            return new Problem.Exceptional(ex);
        }

        return Unit.Instance;
    }

    private static string CreateDeploymentImagePatchConfig(string deploymentName, string imagePath, string imageVersion) =>
        $$"""
        {
          "spec": {
            "template": {
              "spec": {
                "containers": [
                  {
                    "name": "{{ deploymentName }}",
                    "image": "{{ imagePath }}:{{ imageVersion }}"
                  }
                ]
              }
            }
          }
        }
        """;

    private static partial class Log
    {
        [LoggerMessage(LogLevel.Error, "Failed to update deployment configuration.")]
        public static partial void DeploymentUpdateFailed(ILogger logger, Exception ex);
    }
}
