using System.Text.Json.Serialization;

namespace HwoodiwissSyncer.Features.GitHub.Events.Models;

public sealed record PackageVersionInfo(
    [property: JsonPropertyName("container_metadata")]
    ContainerMetadata? ContainerMetadata,
    [property: JsonPropertyName("package_url")]
    string PackageUrl
    );
