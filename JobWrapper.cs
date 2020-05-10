using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Scheduler
{
    public class JobWrapper 
    {
        private IJob job;
        private ConcurrentDictionary<Guid, ICancellableJobItem> scheduledJobItems;

        public JobWrapper(IJob job, ConcurrentDictionary<Guid, ICancellableJobItem> scheduledJobItems)
        {
            this.job = job;
            this.scheduledJobItems = scheduledJobItems;
        }

        public async Task<JobCompletionStatus> RunAsync(ICancellableJobItem jobItem, CancellationToken token)
        {
            try
            {
                await this.job.RunAsync(jobItem.JobItem, token);
                this.RemoveScheduledJobItem(jobItem);
                return JobCompletionStatus.Successful;
            }
            catch (TaskCanceledException)
            {
                this.RemoveScheduledJobItem(jobItem);
                return JobCompletionStatus.Canceled;
            }
            catch(Exception ex)
            {
                jobItem.IncrementFailureCount();
                return JobCompletionStatus.Failed;
            }
        }

        private void RemoveScheduledJobItem(ICancellableJobItem jobItem)
        {
            this.scheduledJobItems.TryRemove(jobItem.Id, out var _);
        }
    }
}
