using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Scheduler
{
    public static class AsyncHelpers
    {
        public static Task ToTask(this WaitHandle waitHandle, TimeSpan timeout)
        {
            var taskCompletionSource = new TaskCompletionSource<object>();
            var registration = ThreadPool.RegisterWaitForSingleObject(
                waitHandle,
                WaitCallback,
                taskCompletionSource,
                timeout,
                true);
            taskCompletionSource.Task.ContinueWith((_, state) => ((RegisteredWaitHandle)state).Unregister(null), registration, TaskScheduler.Default);
            return taskCompletionSource.Task;
        }

        public static Task ToTask(this WaitHandle waitHandle, TimeSpan timeout, TimeSpan delay)
        {
            return Task.Delay(delay).ContinueWith(_ => waitHandle.ToTask(timeout));
        }

        private static void WaitCallback(object state, bool isTimedOut)
        {
            var tcs = state as TaskCompletionSource<object>;
            if (isTimedOut)
            {
                tcs.TrySetException(new TimeoutException("The operation timed out"));
            }
            else
            {
                tcs.TrySetResult(null);
            }
        }
    }
}
