using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Scheduler
{
    // This is a decorator therefore it should forward everything to the nested job item
    public class CancellableJobItem : ICancellableJobItem
    {
        private readonly IJobItem jobItem;
        private readonly CancellationTokenSource cancellationTokenSource;
        private int failureCount;
        private int isRunning;

        public CancellableJobItem(IJobItem jobItem, CancellationToken schedulerCancellationToken)
        {
            this.jobItem = jobItem;
            this.cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(schedulerCancellationToken);
        }

        public Guid Id => this.jobItem.Id;

        public int FailureCount => this.failureCount;

        public IJobItem JobItem => this.jobItem;

        public CancellationToken CancellationToken => cancellationTokenSource.Token;

        public bool IsRunning => Interlocked.CompareExchange(ref this.isRunning, 1, 1)  == 1;

        public void Cancel()
        {
            this.cancellationTokenSource.Cancel();
        }

        public void IncrementFailureCount()
        {
            Interlocked.Increment(ref failureCount);
        }

        public void SetInProgress()
        {
            Interlocked.CompareExchange(ref this.isRunning, 1, 0);
        }
    }
}
