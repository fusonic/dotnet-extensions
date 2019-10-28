using System;

namespace Fusonic.Extensions.UnitTests
{
    /// <summary>
    /// Event args for the UnitTest.ResolveRequest-event. 
    /// </summary>
    public class ResolveTypeRequestEventArgs : EventArgs
    {
        public ResolveTypeRequestEventArgs(Type type)
        {
            Type = type;
        }

        /// <summary> Resolved instance. If this is set further event registrations won't be processed. </summary>
        public object? Instance { get; set; }

        /// <summary> Type that should be resolved. </summary>
        public Type Type { get; }
    }
}