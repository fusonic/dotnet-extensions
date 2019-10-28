using Microsoft.Extensions.Logging;

namespace Fusonic.Extensions.UnitTests.Logging
{
    /// <summary>
    /// Implementation of the Microsoft ILoggerProvider. Can be used for several services, for example for EF logs.
    /// Logs to the ITestOutputHelper configured in the TestContext.
    /// </summary>
    public class XunitLoggerProvider : ILoggerProvider
    {
        public ILogger CreateLogger(string categoryName)
        {
            return new XunitLogger(categoryName);
        } 

        public void Dispose()
        { }
    }
}
