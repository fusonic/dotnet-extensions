using System.Threading;

namespace Fusonic.Extensions.UnitTests.Adapters.EntityFrameworkCore
{
    internal static class DatabaseTestContext
    {
        public static AsyncLocal<DatabaseProviderAttribute?> CurrentProviderAttribute = new AsyncLocal<DatabaseProviderAttribute?>();
    }
}