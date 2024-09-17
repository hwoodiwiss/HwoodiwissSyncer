using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace HwoodiwissSyncer.Features.GitHub.Events.Models;

public sealed record ContainerMetadataTag(
    [property: JsonPropertyName("name")]
    string Name,
    [property: JsonPropertyName("digest")]
    string Digest
    );
