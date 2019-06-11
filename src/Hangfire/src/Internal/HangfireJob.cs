using System.Globalization;

namespace Fusonic.Extensions.Hangfire.Internal
{
    public class HangfireJob
    {
        public object Message { get; set; }
        public string HandlerType { get; set; }
        public CultureInfo Culture { get; set; }
        public CultureInfo UiCulture { get; set; }
    }
}