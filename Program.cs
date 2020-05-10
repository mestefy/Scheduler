using System;

namespace Scheduler
{
    class Program
    {
        static void Main(string[] args)
        {
            var scheduler = new JobScheduler(new FileOperationsJobFactory());

            for (var index = 0; index < 10; index++)
            {
                scheduler.AddJob(new FileJobItem { Id= Guid.NewGuid(), FilePath = $"JobFile{index}" });
            }

            Console.WriteLine("Jobbing away!");
            Console.ReadKey();

            Console.WriteLine("Stopping!");
            scheduler.Stop();

            Console.WriteLine("END!");
            Console.ReadKey();
        }
    }
}
