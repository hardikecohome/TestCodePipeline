using System;
using System.Collections.Generic;
using Hangfire;
using Hangfire.Storage;
using Hangfire.Storage.Monitoring;

namespace DealnetPortal.Api
{
    public static class HangfireExtensions
    {
        public static void DeactivateAllJobs(this IMonitoringApi monitor)
        {
            var toDelete = new List<string>();

            foreach (QueueWithTopEnqueuedJobsDto queue in monitor.Queues())
            {
                for (var i = 0; i < Math.Ceiling(queue.Length / 1000d); i++)
                {
                    monitor.EnqueuedJobs(queue.Name, 1000 * i, 1000)
                        .ForEach(x => toDelete.Add(x.Key));
                   
                }
            }
            monitor.ScheduledJobs(0, (int)monitor.ScheduledCount())
                      .ForEach(x => toDelete.Add(x.Key));
            foreach (var jobId in toDelete)
            {
                BackgroundJob.Delete(jobId);
            }
        }
    }
}