using HwoodiwissSyncer.Extensions;

var app = WebApplication
    .CreateSlimBuilder(args)
    .ConfigureAndBuild();

await app
    .ConfigureRequestPipeline()
    .RunAsync();

namespace HwoodiwissSyncer
{
    public partial class Program;
}
