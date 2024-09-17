namespace HwoodiwissSyncer.Extensions;

public static class IConfigurationBuilderExtensions
{
    public static IConfigurationBuilder ConfigureConfiguration(this IConfigurationBuilder configurationBuilder) =>
        configurationBuilder
            .AddUserSecrets<Program>();
}
