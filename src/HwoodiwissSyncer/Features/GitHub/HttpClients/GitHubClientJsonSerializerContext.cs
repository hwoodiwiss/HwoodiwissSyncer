﻿using System.Text.Json.Serialization;

namespace HwoodiwissSyncer.Features.GitHub.HttpClients;

[JsonSerializable(typeof(InstallationTokenRequest))]
[JsonSerializable(typeof(InstallationTokenResponse))]
[JsonSerializable(typeof(SubmitReviewRequest))]
public sealed partial class GitHubClientJsonSerializerContext : JsonSerializerContext;
