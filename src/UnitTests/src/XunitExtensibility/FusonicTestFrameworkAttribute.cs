using System;
using Xunit.Sdk;

namespace Fusonic.Extensions.UnitTests.XunitExtensibility
{
    /// <summary>
    /// Used to decorate an assembly to allow the use of a custom <see cref="T:Xunit.Sdk.ITestFramework"/>.
    /// </summary>
    [TestFrameworkDiscoverer("Fusonic.Extensions.UnitTests.XunitExtensibility." + nameof(FusonicTestFrameworkTypeDiscoverer), "Fusonic.Extensions.UnitTests")]
    [AttributeUsage(AttributeTargets.Assembly)]
    public sealed class FusonicTestFrameworkAttribute : Attribute, ITestFrameworkAttribute
    { }
}