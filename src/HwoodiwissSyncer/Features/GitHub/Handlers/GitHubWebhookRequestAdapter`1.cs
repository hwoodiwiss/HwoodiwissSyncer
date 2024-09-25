using System.Diagnostics;
using HwoodiwissSyncer.Features.GitHub.Events;
using HwoodiwissSyncer.Handlers;
using OpenTelemetry.Trace;

namespace HwoodiwissSyncer.Features.GitHub.Handlers;

public sealed partial class GithubWebhookRequestAdapter<TEvent, TCommand>(
    ILogger<GithubWebhookRequestAdapter<TEvent, TCommand>> logger,
    IMapper<TEvent, TCommand> mapper,
    IRequestHandler<TCommand> requestHandler,
    ActivitySource activitySource) : IRequestHandler<GitHubWebhookEvent>
    where TEvent : GitHubWebhookEvent
{
    private Type EventType { get; } = typeof(TEvent);
    private Type CommandType { get; } = typeof(TCommand);
    private ActivitySource ActivitySource { get; } = activitySource;

    public async ValueTask<object?> HandleAsync(GitHubWebhookEvent request)
    {
        using var activity = ActivitySource.StartActivity("Handling Github Webhook Event");
        activity?.SetTag("event.repository", EventType.Name);
        activity?.SetTag("event.type", EventType.Name);
        activity?.SetTag("command.type", CommandType.Name);
        activity?.SetTag("event.handler", requestHandler.GetType().Name);
        activity?.SetTag("event.sender.login", request.Sender.Login);
        activity?.SetTag("event.sender.type", request.Sender.Type);
        activity?.SetTag("event.installation.id", request.Installation.Id);

        Log.HandlingEvent(logger, EventType);

        if (request is not TEvent matchingRequestType)
        {
            throw new InvalidOperationException("The provided event type did not match the required type for the current handler.");
        }

        return mapper.Map(matchingRequestType) switch
        {
            Result<TCommand>.Success { Value: var mappedValue } => await requestHandler.HandleAsync(mappedValue),
            Result<TCommand>.Failure failure => await HandleMappingFailureAsync(failure, activity),
            _ => throw new UnreachableException(),
        };
    }

    private ValueTask<object?> HandleMappingFailureAsync(Result<TCommand>.Failure failure, Activity? activity)
    {
        activity?.SetStatus(Status.Error);

        if (failure.Problem is Problem.Exceptional { Value: var exception })
        {
            activity?.RecordException(exception);
            Log.FailureMappingEventToCommandException(logger, EventType, CommandType, exception);
        }

        if (failure.Problem is Problem.Reason { Value: var reason })
        {
            Log.FailureMappingEventToCommand(logger, EventType, CommandType, reason);
        }

        return new ValueTask<object?>((object?)null);
    }

    private static partial class Log
    {
        [LoggerMessage(LogLevel.Information, "Handling webhook event for {EventType}")]
        public static partial void HandlingEvent(ILogger logger, Type eventType);

        [LoggerMessage(LogLevel.Error, "Failure mapping event of type {EventType} to {CommandType} because {Reason}")]
        public static partial void FailureMappingEventToCommand(ILogger logger, Type eventType, Type commandType, string reason);

        [LoggerMessage(LogLevel.Error, "Failure mapping event of type {EventType} to {CommandType} with Exception")]
        public static partial void FailureMappingEventToCommandException(ILogger logger, Type eventType, Type commandType, Exception ex);
    }
}
