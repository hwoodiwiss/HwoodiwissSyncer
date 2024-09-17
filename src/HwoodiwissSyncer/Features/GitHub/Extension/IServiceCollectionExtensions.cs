using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using HwoodiwissSyncer.Features.GitHub.Commands;
using HwoodiwissSyncer.Features.GitHub.Configuration;
using HwoodiwissSyncer.Features.GitHub.Events;
using HwoodiwissSyncer.Features.GitHub.Handlers;
using HwoodiwissSyncer.Features.GitHub.HttpClients;
using HwoodiwissSyncer.Features.GitHub.Mappers;
using HwoodiwissSyncer.Features.GitHub.Services;
using HwoodiwissSyncer.Handlers;

namespace HwoodiwissSyncer.Features.GitHub.Extension;

public static class IServiceCollectionExtensions
{
    public static IServiceCollection ConfigureGitHubServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<GitHubConfiguration>(configuration.GetSection(GitHubConfiguration.SectionName));
        services.Configure<DeploymentConfiguration>(configuration);
        
        services.PostConfigure<GitHubConfiguration>(config =>
        {
            var approxDecodedLength = config.AppPrivateKey.Length / 4 * 3; // Base64 is roughly 4 bytes per 3 chars
            Span<byte> buffer = approxDecodedLength < 2000 ? stackalloc byte[approxDecodedLength] : new byte[approxDecodedLength];
            if (Convert.TryFromBase64String(config.AppPrivateKey, buffer, out var bytesWritten))
            {
                config.AppPrivateKey = Encoding.UTF8.GetString(buffer[..bytesWritten]);
            }
        });
        
        services.AddSingleton<IGitHubSignatureValidator, GitHubSignatureValidator>();
        services.AddSingleton<IGitHubAppAuthProvider, GitHubAppAuthProvider>();
        services.AddScoped<IGitHubService, GitHubService>();
        services.AddGitHubWebhookHandlers();

        services.AddHttpClient<IGitHubClient, GitHubClient>(client =>
        {
            client.BaseAddress = new Uri("https://api.github.com");
            client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("HwoodiwissSyncer", $"{ApplicationMetadata.Version}+{ApplicationMetadata.GitCommit}"));
        });

        return services;
    }
    
    private static IServiceCollection AddGitHubWebhookHandlers(this IServiceCollection services)
    {
        services.AddGitHubEventHandler<PackagePublishedHandler, RegistryPackage.Published, UpdateDeploymentImageCommand, UpdateDeploymentImageCommandMapper>();
        return services;
    }
    
    private static IServiceCollection AddGitHubEventHandler<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] THandler,
        TEvent,
        TCommand,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TMapper>(this IServiceCollection services)
        where TEvent : GitHubWebhookEvent
        where THandler : GithubWebhookRequestHandler<TEvent, TCommand>
        where TMapper : class, IMapper<TEvent, TCommand>
    {
        services.AddSingleton<IMapper<TEvent, TCommand>, TMapper>();
        services.AddKeyedScoped<IRequestHandler<GitHubWebhookEvent>, THandler>(typeof(TEvent));
        return services;
    }
}
