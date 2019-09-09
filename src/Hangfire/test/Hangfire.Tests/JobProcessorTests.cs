using System.Globalization;
using System.Threading.Tasks;
using SimpleInjector;
using Xunit;

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
            await GetInstance<JobProcessor>().ProcessAsync(new HangfireJob(new Command(), typeof(CommandHandler).AssemblyQualifiedName!));
        }

        [Fact]
        public async Task CanProcessNotification()
        {
            await GetInstance<JobProcessor>().ProcessAsync(new HangfireJob(new Notification(), typeof(NotificationHandler).AssemblyQualifiedName!));
        }

        [Fact]
        public async Task CanProcessNotificationSync()
        {
            await GetInstance<JobProcessor>().ProcessAsync(new HangfireJob(new Notification(), typeof(SyncNotificationHandler).AssemblyQualifiedName!));
        }

        [Fact]
        public async Task CanProcessRequest()
        {
            await GetInstance<JobProcessor>().ProcessAsync(new HangfireJob(new Request(), typeof(RequestHandler).AssemblyQualifiedName!));
        }

        [Fact]
        public async Task CanProcessRequestSync()
        {
            await GetInstance<JobProcessor>().ProcessAsync(new HangfireJob(new SyncRequest(), typeof(SyncRequestHandler).AssemblyQualifiedName!));
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

            await GetInstance<JobProcessor>().ProcessAsync(new HangfireJob(new CommandWithCulture(), typeof(CommandHandlerWithCulture).AssemblyQualifiedName!));
        }
    }
}