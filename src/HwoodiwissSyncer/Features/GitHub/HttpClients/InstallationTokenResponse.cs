using System.Text.Json.Serialization;

namespace HwoodiwissSyncer.Features.GitHub.HttpClients;

public class InstallationTokenResponse
{
    [JsonPropertyName("token")]
    public required string Token { get; init; }

    [JsonPropertyName("expires_at")]
    public required DateTimeOffset ExpiresAt { get; init; }
}
