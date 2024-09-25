using System.Diagnostics;

namespace HwoodiwissSyncer.Extensions;

public static class ResultExtensions
{
    public static async Task<Result<TOut>> ThenAsync<TIn, TOut>(this Task<Result<TIn>> result, Func<TIn, Task<Result<TOut>>> thenAsync) =>
        await result switch
        {
            Result<TIn>.Success success => await thenAsync(success.Value),
            Result<TIn>.Failure failure => failure.Problem,
            _ => throw new UnreachableException(),
        };

    public static async Task<Result<TOut>> ThenAsync<TIn, TOut>(this Result<TIn> result, Func<TIn, Task<Result<TOut>>> thenAsync) =>
        result switch
        {
            Result<TIn>.Success success => await thenAsync(success.Value),
            Result<TIn>.Failure failure => failure.Problem,
            _ => throw new UnreachableException(),
        };
}
