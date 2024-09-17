﻿namespace HwoodiwissSyncer.Handlers;

public interface IRequestHandler<in T>
{
    public ValueTask<object?> HandleAsync(T request);
}
