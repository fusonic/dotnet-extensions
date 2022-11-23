// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Microsoft.Extensions.Logging;

namespace Fusonic.Extensions.XUnit.Logging;

/// <summary>
/// Implementation of the Microsoft ILogger. Can be used for several services, for example for EF logs.
/// Logs to the ITestOutputHelper configured in the TestContext.
/// </summary>
public class XUnitLogger : ILogger
{
    private readonly string categoryName;

    public XUnitLogger(string categoryName) => this.categoryName = categoryName;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        TestContext.WriteLine($"{categoryName} [{eventId}] {formatter(state, exception)}");
        if (exception != null)
            TestContext.WriteLine(exception.ToString());
    }

    public bool IsEnabled(LogLevel logLevel) => true;
    public IDisposable BeginScope<TState>(TState state) where TState : notnull => NoScope.Instance;

    private sealed class NoScope : IDisposable
    {
        public static readonly NoScope Instance = new();
        public void Dispose()
        { }
    }
}
