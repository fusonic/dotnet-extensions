using System;
using System.Globalization;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Server;
using Hangfire.Storage;
using SimpleInjector;
using Xunit;
using NSubstitute;

namespace Fusonic.Extensions.Hangfire.Tests
{
    public class HangfireJobProcessorTests
    {
        private Container Container { get; } = new Container();

        protected T GetInstance<T>()
            where T : class
        {
            return Container.GetInstance<T>();
        }

        [Fact]
        public async Task CanProcessCommand()
        {
            await GetInstance<JobProcessor>().ProcessAsync(new MediatorHandlerContext(new Command(), typeof(CommandHandler).AssemblyQualifiedName!), CreatePerformContext());
        }

        [Fact]
        public async Task CanProcessNotification()
        {
            await GetInstance<JobProcessor>().ProcessAsync(new MediatorHandlerContext(new Notification(), typeof(NotificationHandler).AssemblyQualifiedName!), CreatePerformContext());
        }

        [Fact]
        public async Task CanProcessNotificationSync()
        {
            await GetInstance<JobProcessor>().ProcessAsync(new MediatorHandlerContext(new Notification(), typeof(SyncNotificationHandler).AssemblyQualifiedName!), CreatePerformContext());
        }

        [Fact]
        public async Task CanProcessRequest()
        {
            await GetInstance<JobProcessor>().ProcessAsync(new MediatorHandlerContext(new Request(), typeof(RequestHandler).AssemblyQualifiedName!), CreatePerformContext());
        }

        [Fact]
        public async Task CanProcessRequestSync()
        {
            await GetInstance<JobProcessor>().ProcessAsync(new MediatorHandlerContext(new SyncRequest(), typeof(SyncRequestHandler).AssemblyQualifiedName!), CreatePerformContext());
        }

        [Fact]
        public async Task RestoreCulture()
        {
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;

            var specific = CultureInfo.CreateSpecificCulture("en-us");
            var specificUI = CultureInfo.CreateSpecificCulture("de-at");

            Container.Register(() => new CommandHandlerWithCulture(() =>
            {
                Assert.Equal(specific, CultureInfo.CurrentCulture);
                Assert.Equal(specificUI, CultureInfo.CurrentUICulture);
            }));

            CultureInfo.CurrentCulture = specific;
            CultureInfo.CurrentUICulture = specificUI;

            await GetInstance<JobProcessor>().ProcessAsync(new MediatorHandlerContext(new CommandWithCulture(), typeof(CommandHandlerWithCulture).AssemblyQualifiedName!), CreatePerformContext());
        }

        private static PerformContext CreatePerformContext()
        {
            return new PerformContext(Substitute.For<JobStorage>(),
                    Substitute.For<IStorageConnection>(),
                    new BackgroundJob(Guid.NewGuid().ToString(), new JobData().Job, DateTime.UtcNow),
                    Substitute.For<IJobCancellationToken>());
        }
    }
}