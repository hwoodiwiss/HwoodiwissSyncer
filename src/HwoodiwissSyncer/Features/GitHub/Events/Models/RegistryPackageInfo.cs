using System.Text.Json.Serialization;

namespace HwoodiwissSyncer.Features.GitHub.Events.Models;

public sealed record RegistryPackageInfo(
    [property: JsonPropertyName("created_at")]
    DateTimeOffset CreatedAt,    
    [property: JsonPropertyName("ecosystem")]
    string Ecosystem,
    [property: JsonPropertyName("id")]
    long Id,     
    [property: JsonPropertyName("name")]
    string Name,     
    [property: JsonPropertyName("package_type")]
    string PackageType,
    [property: JsonPropertyName("package_version")]
    PackageVersionInfo PackageVersion
    );
