// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Aspire.Hosting.ApplicationModel;

namespace Fusonic.Extensions.Aspire.Hosting;

/// <summary>The container's service name in the Compose stack; <see langword="null"/> leaves it out of the stack.</summary>
internal sealed record ComposeServiceAnnotation(string? ServiceName) : IResourceAnnotation;
