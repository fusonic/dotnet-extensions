// // Copyright (c) Fusonic GmbH. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for license information.

using Xunit.Abstractions;
using Xunit.Sdk;

namespace Fusonic.Extensions.XUnit.Framework;

/// <summary>
/// Used to decorate an assembly to allow the use of a custom <see cref="ITestFramework"/>.
/// </summary>
[TestFrameworkDiscoverer("Fusonic.Extensions.XUnit.Framework." + nameof(FusonicTestFrameworkTypeDiscoverer), "Fusonic.Extensions.XUnit")]
[AttributeUsage(AttributeTargets.Assembly)]
public sealed class FusonicTestFrameworkAttribute : Attribute, ITestFrameworkAttribute
{
    /// <summary>
    /// Gets the maximum number of tests running in parallel.
    /// When using MaxParallelThreads from XUnit, it limits the number of maximum <i>active</i> tests executing, but it does not limit of maximum parallel tests started.
    /// As soon as a test awaits a task somewhere, the thread is returned to the pool and another test gets started. This is intended by design.<br/>
    /// This option limits the number of tests that get started in parallel.
    /// If set to 0, the system will use <see cref="Environment.ProcessorCount" />.
    /// If set to a negative number, then there will be no limit to the number of tests.
    /// Defaults to 0.
    /// </summary>
    public int MaxParallelTests { get; set; }
}