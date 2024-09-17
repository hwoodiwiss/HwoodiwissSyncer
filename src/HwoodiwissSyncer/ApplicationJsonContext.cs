using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using HwoodiwissSyncer.Features.GitHub.Events;

namespace HwoodiwissSyncer;

[JsonSerializable(typeof(object))]
[JsonSerializable(typeof(JsonObject))]
[JsonSerializable(typeof(KeyValuePair<string, string>))]
[JsonSerializable(typeof(Dictionary<string, string>))]
[JsonSerializable(typeof(RegistryPackage))]
[JsonSerializable(typeof(Unit))]
public partial class ApplicationJsonContext : JsonSerializerContext;

