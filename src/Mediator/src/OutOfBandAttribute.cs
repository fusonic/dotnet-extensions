// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace Fusonic.Extensions.Mediator;

/// <summary>
/// Indicates that the decorated class can run out-of-band of the current flow.
/// Out-of-band means that the actual class may be enqueued into a queuing system for async exceution.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class OutOfBandAttribute : Attribute
{ }
