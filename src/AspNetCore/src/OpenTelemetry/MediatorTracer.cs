// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using System.Diagnostics;

namespace Fusonic.Extensions.AspNetCore.OpenTelemetry;

internal static class MediatorTracer
{
    public const string ActivitySourceName = "Mediator";
    public static readonly ActivitySource MediatorActivitySource = new(ActivitySourceName);

    public static async Task<T> TraceRequest<T>(Type handlerType, string displayName, string kind, Func<Task<T>> handle)
    {
        using var activity = MediatorActivitySource.StartActivity(kind, ActivityKind.Internal, null);
        if (activity != null)
        {
            activity.DisplayName = displayName;
            activity.SetTag("handler.type", GetTypeName(handlerType));
            activity.SetTag("handler.kind", kind);
        }

        try
        {
            return await handle();
        }
        catch (Exception ex)
        {
            if (activity != null)
            {
                activity.SetStatus(ActivityStatusCode.Error, ex.Message);
                activity.AddException(ex);
            }

            throw;
        }
    }

    public static string GetTypeName(Type type) => type.FullName ?? type.Name;
}