using System;

namespace Fusonic.Extensions.UnitTests.Adapters.EntityFrameworkCore
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class NoDatabaseAttribute : DatabaseProviderAttribute
    { }
}