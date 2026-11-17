// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Timer = System.Timers.Timer;

namespace Fusonic.Extensions.Hosting.TimedHostedService;

/// <summary> Executes a service repeatedly in a specified interval, independent of the used DI container. </summary>
public abstract class TimedHostedServiceBase<TService> : IHostedService, IDisposable
    where TService : class
{
    private readonly ILogger logger;
    private readonly Settings settings;
    private readonly IHttpClientFactory httpClientFactory;
    private readonly Timer timer;

    private CancellationTokenSource? tokenSource;
    private CancellationToken stopToken = CancellationToken.None;

    public Task ExecutingTask { get; private set; } = Task.CompletedTask;

    protected TimedHostedServiceBase(ILogger logger, Settings settings, IHttpClientFactory httpClientFactory)
    {
        this.logger = logger;
        this.settings = settings;
        this.httpClientFactory = httpClientFactory;

        timer = new Timer
        {
            Interval = settings.Interval.TotalMilliseconds,
            AutoReset = false
        };
        timer.Elapsed += (_, _) => ExecutingTask = StartExecuteTask();
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        tokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        stopToken = tokenSource.Token;

        // Run the task in the background immediately. The tasks starts the timer once it completes.
        ExecutingTask = StartExecuteTask();

        logger.LogInformation("TimedHostedService {Type} started.", typeof(TService).Name);
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Stopping TimedHostedService {Type}...", typeof(TService).Name);

        try
        {
            tokenSource?.Cancel();
        }
        finally
        {
            timer.Stop();

            // Wait until the task completes or the stop token triggers
            await Task.WhenAny(ExecutingTask, Task.Delay(Timeout.Infinite, cancellationToken));
        }

        logger.LogInformation("TimedHostedService {Type} stopped.", typeof(TService).Name);
    }

    private async Task StartExecuteTask()
    {
        try
        {
            if (stopToken.IsCancellationRequested)
                return;

            logger.LogInformation("Executing task for TimedHostedService {Type}", typeof(TService).Name);

            await RunInScopeAsync(service => settings.ExecuteTask(service, stopToken));

            await Woof();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error when running task for TimedHostedService {Type}.", typeof(TService).Name);
        }
        finally
        {
            timer.Start();
        }
    }

    private async Task Woof()
    {
        if (settings.WatchdogUri != null)
        {
            logger.LogDebug("Woof");
            await httpClientFactory.CreateClient($"TimedHostedService_{typeof(TService).Name}_Watchdog")
                                   .GetStringAsync(settings.WatchdogUri, stopToken);
        }
    }

    /// <summary> Resolves the service within a new scope and runs the <paramref name="action"/> on it. </summary>
    protected abstract Task RunInScopeAsync(Func<TService, Task> action);

    public void Dispose()
    {
        timer.Dispose();
        tokenSource?.Dispose();
        GC.SuppressFinalize(this);
    }

    public class Settings
    {
        /// <summary> Interval in which the sync gets started. </summary>
        public TimeSpan Interval { get; }

        /// <summary> Url that gets notified that the service is still alive. </summary>
        public Uri? WatchdogUri { get; }

        /// <summary> Action that gets executed on the service </summary>
        public Func<TService, CancellationToken, Task> ExecuteTask { get; }

        public Settings(TimeSpan interval, Uri? watchdogUri, Func<TService, CancellationToken, Task> executeTask)
        {
            if (interval == TimeSpan.Zero)
                throw new ArgumentException("Invalid configuration: the interval is zero.");
            if (interval < TimeSpan.Zero)
                throw new ArgumentException("Invalid configuration: the interval is negative.");

            Interval = interval;
            ExecuteTask = executeTask ?? throw new ArgumentNullException(nameof(executeTask));
            WatchdogUri = watchdogUri;
        }
    }
}
