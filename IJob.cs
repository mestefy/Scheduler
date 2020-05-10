using System.Threading;
using System.Threading.Tasks;

namespace Scheduler
{
    public interface IJob
    {
        Task RunAsync(IJobItem jobItem, CancellationToken token);
    }
}
