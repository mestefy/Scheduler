using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Scheduler
{
    public class JobScheduler
    {
        private readonly ConcurrentDictionary<Guid, ICancellableJobItem> scheduledJobs = new ConcurrentDictionary<Guid, ICancellableJobItem>();
        private readonly CancellationTokenSource schedulerCancellationTokenSource = new CancellationTokenSource();
        private readonly IJobFactory jobFactory;
        private readonly ManualResetEvent manualResetEvent = new ManualResetEvent(false);

        private const int MaxParallelJobs = 2;

        public JobScheduler(IJobFactory jobFactory)
        {
            this.jobFactory = jobFactory;
            Task.Factory.StartNew(async () => await this.RunSchedulerAsync(schedulerCancellationTokenSource.Token), TaskCreationOptions.LongRunning);
        }

        public bool AddJob(IJobItem jobItem)
        {
            var cancellableJobItem = this.CreateCancellableJobItem(jobItem);
            var added = this.scheduledJobs.TryAdd(jobItem.Id, cancellableJobItem);
            if (added)
            {
                this.manualResetEvent.Set();
            }

            return added;
        }

        public void RemoveJob(Guid jobItemId)
        {
            if (this.scheduledJobs.TryRemove(jobItemId, out var jobItem))
            {
                jobItem.Cancel();
            }
        }

        public void Stop()
        {
            this.scheduledJobs.Clear();
            this.schedulerCancellationTokenSource.Cancel();
            this.manualResetEvent.Set();
        }

        private async Task RunSchedulerAsync(CancellationToken cancellationToken)
        {
            var jobs = new List<Task<JobCompletionStatus>>();

            // Looping while the scheduler should run
            while (!cancellationToken.IsCancellationRequested)
            {
                this.manualResetEvent.Reset();

                // Looping while there are available jobs
                while (!this.scheduledJobs.IsEmpty)
                {
                    Console.WriteLine($"No of running jobs [{jobs.Count}]");

                    foreach (var jobItem in this.scheduledJobs.Values.Where(item => !item.IsRunning).OrderBy(item => item.FailureCount).Take(MaxParallelJobs - jobs.Count))
                    {
                        var job = this.CreateJob();
                        jobItem.SetInProgress();

                        jobs.Add(job.RunAsync(jobItem, jobItem.CancellationToken));
                    }

                    var completed = await Task.WhenAny(jobs);
                    jobs.Remove(completed);
                }

                // wait for single object
                await this.manualResetEvent.ToTask(Timeout.InfiniteTimeSpan);
            }
        }

        private ICancellableJobItem CreateCancellableJobItem(IJobItem jobItem)
        {
            return new CancellableJobItem(jobItem, this.schedulerCancellationTokenSource.Token);
        }

        private JobWrapper CreateJob()
        {
            return new JobWrapper(this.jobFactory.CreateJob(), this.scheduledJobs);
        }
    }
}
