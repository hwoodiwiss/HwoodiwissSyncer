using Hwoodiwiss.Extensions.Hosting;
using HwoodiwissSyncer;
using HwoodiwissSyncer.Features.GitHub.Endpoints;
using HwoodiwissSyncer.Features.GitHub.Extension;
using HwoodiwissSyncer.Features.Kubernetes.Extensions;

var builder = HwoodiwissApplication.CreateBuilder(args)
    .WithHttpJsonContexts(ApplicationJsonContext.Default)
    .ConfigureOptions(opt => opt.HostStaticAssets = true);

builder.Services.ConfigureGitHubServices(builder.Configuration);
builder.Services.ConfigureKubernetesServices(builder.Configuration);

var app = builder.Build();

await app
    .MapGitHubEndpoints()
    .RunAsync();

namespace HwoodiwissSyncer
{
    public partial class Program;
}
