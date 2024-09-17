namespace HwoodiwissSyncer.Extensions;

public static class IConfigurationBuilderExtensions
{
    public static IConfigurationBuilder ConfigureConfiguration(this IConfigurationBuilder configurationBuilder) =>
        configurationBuilder
            .AddJsonFile("appsettings.Secrets.json")
            .AddUserSecrets<Program>();
}
