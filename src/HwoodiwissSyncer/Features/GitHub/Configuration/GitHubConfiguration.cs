﻿using HwoodiwissSyncer.Configuration;

namespace HwoodiwissSyncer.Features.GitHub.Configuration;

public sealed class GitHubConfiguration : INamedConfiguration
{
    public static string SectionName => "Github";

    public required string WebhookKey { get; set; }

    public required bool EnableRequestLogging { get; set; }

    public required string AppId { get; set; }

    public required string AppPrivateKey { get; set; }

    public required string[] AllowedBots { get; set; }
}
