// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using System.Threading.Tasks;
using Hangfire.Server;

namespace Fusonic.Extensions.Hangfire
{
    public interface IJobProcessor
    {
        Task ProcessAsync(MediatorHandlerContext context, PerformContext performContext);
    }
}