using System.Text.Json.Serialization;

namespace HwoodiwissSyncer.Features.GitHub.HttpClients;

public sealed record CreateIssueCommentRequest
{
    [JsonPropertyName("body")]
    public required string Body { get; init; }
}
