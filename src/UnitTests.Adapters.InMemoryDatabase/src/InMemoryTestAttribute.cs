using System;
using Fusonic.Extensions.UnitTests.Adapters.EntityFrameworkCore;

namespace Fusonic.Extensions.UnitTests.Adapters.InMemoryDatabase
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class InMemoryTestAttribute : DatabaseProviderAttribute
    { }
}
