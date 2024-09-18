using HwoodiwissSyncer.Features.Kubernetes.Services;
using k8s;

namespace HwoodiwissSyncer.Features.Kubernetes.Extensions;

public static class IServiceCollectionExtensions
{
    public static IServiceCollection ConfigureKubernetesServices(this IServiceCollection services, IConfigurationRoot configurationRoot)
    {
        services.AddSingleton(GetKubernetesConfig());
        services.AddScoped<IKubernetes, k8s.Kubernetes>(sp => new k8s.Kubernetes(sp.GetRequiredService<KubernetesClientConfiguration>()));
        services.AddScoped<IKubernetesService, KubernetesService>();

        return services;
    }
    
    private static KubernetesClientConfiguration GetKubernetesConfig()
    {
        if (KubernetesClientConfiguration.IsInCluster())
        {
            return KubernetesClientConfiguration.InClusterConfig();
        }

        var profileFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        if (File.Exists(Path.Join(profileFolder, ".kube", "config")))
        {
            return KubernetesClientConfiguration.BuildConfigFromConfigFile();
        }

        throw new InvalidOperationException("Failed to load kubernetes config.");
    }
}
