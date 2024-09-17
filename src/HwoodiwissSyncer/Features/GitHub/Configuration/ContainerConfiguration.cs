using System.Text.RegularExpressions;

namespace HwoodiwissSyncer.Features.GitHub.Configuration;

public sealed class ContainerConfiguration
{
    public ICollection<string> LabelPatterns { get; init; } = [];

    public string Image { get; set; } = string.Empty;

    public string Namespace { get; set; } = string.Empty;
}
