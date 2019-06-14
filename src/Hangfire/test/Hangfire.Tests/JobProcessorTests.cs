using System.Globalization;
using System.Threading.Tasks;
using Fusonic.Extensions.Hangfire;
using SimpleInjector;
using Xunit;

namespace Hangfire.Tests
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
            await GetInstance<JobProcessor>().ProcessAsync(new HangfireJob()
            {
                Message = new Command(),
                HandlerType = typeof(CommandHandler).AssemblyQualifiedName
            });
        }

        [Fact]
        public async Task CanProcessNotification()
        {
            await GetInstance<JobProcessor>().ProcessAsync(new HangfireJob()
            {
                Message = new Notification(),
                HandlerType = typeof(NotificationHandler).AssemblyQualifiedName
            });
        }

        [Fact]
        public async Task CanProcessNotificationSync()
        {
            await GetInstance<JobProcessor>().ProcessAsync(new HangfireJob()
            {
                Message = new Notification(),
                HandlerType = typeof(SyncNotificationHandler).AssemblyQualifiedName
            });
        }

        [Fact]
        public async Task CanProcessRequest()
        {
            await GetInstance<JobProcessor>().ProcessAsync(new HangfireJob()
            {
                Message = new Request(),
                HandlerType = typeof(RequestHandler).AssemblyQualifiedName
            });
        }

        [Fact]
        public async Task CanProcessRequestSync()
        {
            await GetInstance<JobProcessor>().ProcessAsync(new HangfireJob()
            {
                Message = new SyncRequest(),
                HandlerType = typeof(SyncRequestHandler).AssemblyQualifiedName
            });
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

            await GetInstance<JobProcessor>().ProcessAsync(new HangfireJob()
            {
                Message = new CommandWithCulture(),
                HandlerType = typeof(CommandHandlerWithCulture).AssemblyQualifiedName
            });
        }
    }
}