using System.Threading.Tasks;
using Hangfire.Server;

namespace Fusonic.Extensions.Hangfire
{
    public interface IJobProcessor
    {
        Task ProcessAsync(MediatorHandlerContext context, PerformContext performContext);
    }
}