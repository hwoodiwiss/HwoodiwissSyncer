﻿using System.Text.Json;
using System.Text.Json.Serialization;

namespace HwoodiwissSyncer.Features.GitHub.Services;

public sealed class UppercaseStringEnumConverter<T>() : JsonStringEnumConverter<T>(JsonNamingPolicy.SnakeCaseUpper)
    where T : struct, Enum;
