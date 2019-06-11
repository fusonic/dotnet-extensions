using System;

namespace Fusonic.Extensions.Abstractions
{
    /// <summary>
    /// Indicates that the decorated class can run out-of-band of the current flow.
    /// Out-of-band means that the actual class may be enqueued into a queuing system for async exceution.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class OutOfBandAttribute : Attribute
    { }
}