using System.Threading.Tasks;

namespace Fusonic.Extensions.Hangfire
{
    public interface IJobProcessor
    {
        Task ProcessAsync(HangfireJob job);
    }
}