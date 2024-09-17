using System.Text.Json.Serialization;

namespace HwoodiwissSyncer.Features.GitHub.Events;

[JsonSerializable(typeof(RegistryPackage))]
public partial class GitHubEventsJsonContext : JsonSerializerContext;
