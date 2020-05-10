using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Scheduler
{
    public class FileOperationsJobFactory : IJobFactory
    {
        public IJob CreateJob()
        {
            return new FileJob();
        }

        // This should be the actual work
        private class FileJob : IJob
        {
            private Random random = new Random();
            //public async Task RunAsync(IJobItem jobItem, CancellationToken token)
            //{
            //    var fileJobItem = jobItem as FileJobItem;
            //    using (var fileStream = File.OpenWrite(fileJobItem.FilePath))
            //    {
            //        Console.WriteLine($"Writing in file [{fileJobItem.FilePath}]");
            //        var buffer = Encoding.UTF8.GetBytes($"Text for file {fileJobItem.FilePath}");
            //        for (var index = 0; index < 10000; index++)
            //        {
            //            await fileStream.WriteAsync(buffer, 0, buffer.Length, token);
            //        }
            //        Console.WriteLine($"Completed writing in file [{fileJobItem.FilePath}]");
            //    }
            //}

            // The stupid version when we create a thread instead of just using the file task
            public Task RunAsync(IJobItem jobItem, CancellationToken token)
            {
                return Task.Run(async () =>
                {
                    var fileJobItem = jobItem as FileJobItem;
                    using (var fileStream = File.OpenWrite(fileJobItem.FilePath))
                    {
                        var buffer = Encoding.UTF8.GetBytes($"Text for file {fileJobItem.FilePath}{Environment.NewLine}");

                        // In order to add some randomness, we'll set randomly the number of lines that should be written
                        var noOfLines = random.Next(10000, 1000000);
                        Console.WriteLine($"Writing in file [{fileJobItem.FilePath}] {noOfLines} lines");


                        for (var index = 0; index < noOfLines; index++)
                        {
                            if (index % 17 == 0)
                            {
                                // adding some randomness
                                // await Task.Delay(10);
                            }
                            await fileStream.WriteAsync(buffer, 0, buffer.Length, token);
                        }
                        Console.WriteLine($"Completed writing in file [{fileJobItem.FilePath}]");
                    }
                });
            }
        }
    }
}
