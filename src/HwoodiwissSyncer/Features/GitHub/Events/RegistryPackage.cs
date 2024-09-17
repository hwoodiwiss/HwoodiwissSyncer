using System.Text.Json.Serialization;
using HwoodiwissSyncer.Features.GitHub.Events.Models;

namespace HwoodiwissSyncer.Features.GitHub.Events;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "action")]
[JsonDerivedType(typeof(Published), "published")]
public abstract record RegistryPackage : GitHubWebhookEvent
{
    public sealed record Published : RegistryPackage
    {
        [JsonPropertyName("repository")]
        public required Repository Repository { get; init; }

        [JsonPropertyName("registry_package")]
        public required RegistryPackageInfo RegistryPackage { get; init; }
    }
}
