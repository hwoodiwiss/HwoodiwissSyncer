using HwoodiwissSyncer.Features.GitHub.Commands;
using HwoodiwissSyncer.Features.GitHub.Events;

namespace HwoodiwissSyncer.Features.GitHub.Mappers;

public class UpdateDeploymentImageCommandMapper : IMapper<RegistryPackage.Published, UpdateDeploymentImageCommand>
{
    public Result<UpdateDeploymentImageCommand> Map(RegistryPackage.Published source)
    {
        if (source.RegistryPackage.PackageType is not "CONTAINER")
        {
            return new Problem.Reason("Package type is not Container");
        }

        if (source.RegistryPackage.PackageVersion.ContainerMetadata?.Tag.Name is not { } tagName)
        {
            return new Problem.Reason("Container tag name was null");
        }

        return new UpdateDeploymentImageCommand(
            tagName,
            source.RegistryPackage.PackageVersion.PackageUrl.Split(':').First(),
            source.Installation.Id,
            source.Repository.Name,
            source.Repository.Owner.Login
        );
    }
}
