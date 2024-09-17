﻿using System.Text.Json;
using HwoodiwissSyncer.Extensions;
using HwoodiwissSyncer.Features.GitHub.Configuration;
using HwoodiwissSyncer.Features.GitHub.Events;
using HwoodiwissSyncer.Handlers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using GitHubSecretValidatorFilter = HwoodiwissSyncer.Features.GitHub.Filters.GitHubSecretValidatorFilter;

namespace HwoodiwissSyncer.Features.GitHub.Endpoints;

public static partial class GitHubWebhookEndpoints
{
    public static IEndpointRouteBuilder MapGitHubEndpoints(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("/github");

        group.MapPost("/webhook", async (
                [FromKeyedServices(nameof(GitHubWebhookEndpoints))] ILogger logger,
                [FromHeader(Name = "X-Github-Event")] string githubEvent,
                [FromServices] IOptions<JsonOptions> jsonOptions,
                HttpRequest request,
                IServiceProvider serviceProvider,
                IOptionsSnapshot<GitHubConfiguration> githubConfiguration) =>
            {
                using var _ = logger.BeginScope(new Dictionary<string, object>
                {
                    ["GithubEvent"] = githubEvent,
                });

                if (githubConfiguration.Value.EnableRequestLogging)
                {
                    var githubEventBody = await GetRequestBodyText(request.Body);
                    Log.ReceivedGithubEvent(logger, githubEventBody);
                    request.Body.Seek(0, SeekOrigin.Begin);
                }

                var githubEventBase = await GetGithubEvent(logger, githubEvent, request.Body);

                var requestHandler = serviceProvider.GetKeyedService<IRequestHandler<GitHubWebhookEvent>>(githubEventBase?.GetType());

                if (githubEventBase is null || requestHandler is null)
                    return Results.NoContent();

                return await requestHandler.HandleAsync(githubEventBase);
            })
            .WithBufferedRequest()
            .AddEndpointFilterFactory(GitHubSecretValidatorFilter.Factory)
            .Produces(201);

        return builder;
    }

    private static async Task<GitHubWebhookEvent?> GetGithubEvent(ILogger logger, string githubEvent, Stream body)
    {
        try
        {
            return githubEvent switch
            {
                "registry_package" => await JsonSerializer.DeserializeAsync(body, GitHubEventsJsonContext.Default.RegistryPackage),
                _ => null,
            };
        }
        catch (JsonException ex)
        {
            var githubEventBody = await GetRequestBodyText(body);
            Log.DeserializationFailed(logger, githubEventBody, ex);
            return null;
        }
        catch (NotSupportedException ex)
        {
            var githubEventBody = await GetRequestBodyText(body);
            Log.DeserializingGithubEventNotSupported(logger, githubEventBody, ex);
            return null;
        }
    }

    private static async Task<string> GetRequestBodyText(Stream requestBody)
    {
        requestBody.Seek(0, SeekOrigin.Begin);
        return await new StreamReader(requestBody).ReadToEndAsync();
    }

    private static partial class Log
    {
        [LoggerMessage(LogLevel.Warning, "Failed to deserialize github event {GithubEventBody}")]
        public static partial void DeserializationFailed(ILogger logger, string githubEventBody, Exception exception);

        [LoggerMessage(LogLevel.Error, "Failed to deserialize github event data {GithubEventBody}")]
        public static partial void DeserializingGithubEventNotSupported(ILogger logger, string githubEventBody, Exception exception);

        [LoggerMessage(LogLevel.Information, "Received Github event: {GithubEventBody}")]
        public static partial void ReceivedGithubEvent(ILogger logger, string githubEventBody);

    }
}
