using System.Text.Json.Serialization;

namespace HwoodiwissSyncer.Features.GitHub.Events.Models;

public sealed record Installation(
    [property: JsonPropertyName("id")]
    int Id,
    [property: JsonPropertyName("node_id")]
    string NodeId);
