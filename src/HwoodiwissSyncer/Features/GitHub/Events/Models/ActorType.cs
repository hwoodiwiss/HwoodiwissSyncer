using System.Text.Json.Serialization;

namespace HwoodiwissSyncer.Features.GitHub.Events.Models;

[JsonConverter(typeof(JsonStringEnumConverter<ActorType>))]
public enum ActorType
{
    Bot,
    User,
    Organization
}
