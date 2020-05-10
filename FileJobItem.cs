using System;

namespace Scheduler
{
    public class FileJobItem : IJobItem
    {
        public Guid Id { get; set; }
        public string FilePath { get; set; }
    }
}
