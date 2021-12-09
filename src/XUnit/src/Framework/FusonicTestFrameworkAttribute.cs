// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Xunit.Abstractions;
using Xunit.Sdk;

namespace Fusonic.Extensions.XUnit.Framework;

/// <summary>
/// Used to decorate an assembly to allow the use of a custom <see cref="ITestFramework"/>.
/// </summary>
[TestFrameworkDiscoverer("Fusonic.Extensions.XUnit.Framework." + nameof(FusonicTestFrameworkTypeDiscoverer), "Fusonic.Extensions.XUnit")]
[AttributeUsage(AttributeTargets.Assembly)]
public sealed class FusonicTestFrameworkAttribute : Attribute, ITestFrameworkAttribute
{ }
