using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Scheduler
{
    public interface ICancellableJobItem : IJobItem
    {
        int FailureCount { get; }

        CancellationToken CancellationToken { get; }

        IJobItem JobItem { get; }


        bool IsRunning { get; }


        void Cancel();

        void SetInProgress();
        void IncrementFailureCount();
    }
}
