using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace HwoodiwissSyncer.Features.GitHub.Events.Models;

public sealed record ContainerMetadata(
    [property: JsonPropertyName("tag")]
    ContainerMetadataTag Tag
    );
