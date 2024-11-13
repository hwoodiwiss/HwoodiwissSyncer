using System.Diagnostics;
using k8s;
using k8s.Models;
using OpenTelemetry.Trace;

namespace HwoodiwissSyncer.Features.Kubernetes.Services;

public sealed partial class KubernetesService(IKubernetes kubeClient, ILogger<KubernetesService> logger, ActivitySource activitySource) : IKubernetesService
{
    public async Task<Result<Unit>> UpdateDeploymentImage(string nameSpace, string deploymentName, string imagePath, string imageVersion)
    {
        using var activity = activitySource.StartActivity("Update deployment image");
        activity?.SetTag("deployment.namespace", nameSpace);
        activity?.SetTag("deployment.name", deploymentName);
        activity?.SetTag("deployment.image", $"{imagePath}:{imageVersion}");

        try
        {
            var deployments = await kubeClient.AppsV1.ListNamespacedDeploymentAsync(nameSpace);

            var matchingDeployment = deployments.Items.FirstOrDefault(w => w.Metadata.Name.Equals(deploymentName));

            var containerSpec = matchingDeployment?.Spec.Template.Spec.Containers.FirstOrDefault(w =>
                w.Image.StartsWith(imagePath, StringComparison.OrdinalIgnoreCase));

            if (containerSpec is not null)
            {
                var patchJson = CreateDeploymentImagePatchConfig(deploymentName, imagePath, imageVersion);
                activity?.SetTag("deployment.patch", patchJson);

                await kubeClient.AppsV1.PatchNamespacedDeploymentAsync(
                    new V1Patch(patchJson, V1Patch.PatchType.StrategicMergePatch),
                    deploymentName,
                    nameSpace);

            }
            else
            {
                return new Problem.Reason($"Could not find matching container spec for {nameSpace}/{deploymentName}");
            }

        }
        catch (Exception ex)
        {
            activity?.AddException(ex);
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
                    "name": "{{deploymentName}}",
                    "image": "{{imagePath}}:{{imageVersion}}",
                    "imagePullPolicy": "Always"
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
