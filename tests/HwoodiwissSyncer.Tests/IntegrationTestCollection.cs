namespace HwoodiwissSyncer.Tests.Integration;

[CollectionDefinition(Name)]
public sealed class IntegrationTestCollection : ICollectionFixture<IntegrationFixture>
{
    public const string Name = nameof(IntegrationTestCollection);
}
