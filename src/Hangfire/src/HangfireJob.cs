using System.Globalization;

namespace Fusonic.Extensions.Hangfire
{
    public class HangfireJob
    {
        public HangfireJob(object message, string handlerType)
        {
            Message = message;
            HandlerType = handlerType;
        }

        public object Message { get; set; }
        public string HandlerType { get; set; }
        public CultureInfo? Culture { get; set; }
        public CultureInfo? UiCulture { get; set; }
    }
}