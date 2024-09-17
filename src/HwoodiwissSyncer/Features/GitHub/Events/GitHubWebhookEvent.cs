using System.Text.Json.Serialization;
using HwoodiwissSyncer.Features.GitHub.Events.Models;

namespace HwoodiwissSyncer.Features.GitHub.Events;

public abstract record GitHubWebhookEvent
{
    [JsonPropertyName("sender")]
    public required Actor Sender { get; init; }

    [JsonPropertyName("installation")]
    public required Installation Installation { get; init; }
}
