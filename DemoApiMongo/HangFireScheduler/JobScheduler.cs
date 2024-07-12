using Hangfire;
using Hangfire.Common;
using Hangfire.Server;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using Quartz;

namespace DemoApiMongo.HangFireScheduler
{
    public class JobScheduler
    {
        private readonly IMongoCollection<HangFireLog> _collection;
        private readonly string _machineIdentifier;
        private static readonly HashSet<string> PausedJobs = new HashSet<string>();

        public JobScheduler(IMongoDatabase database)
        {
            _collection = database.GetCollection<HangFireLog>("HangFireLog1");
            _machineIdentifier = Environment.MachineName;
        }

        public void WriteToTextFile()
        {
            var baseDirectory = Path.Combine(Directory.GetCurrentDirectory(), "SchedulerLogsFile");
            string filePath = Path.Combine(baseDirectory, $"Hangfire_{DateTime.Now:yyyyMMdd}.txt");

            if (!Directory.Exists(baseDirectory))
            {
                Directory.CreateDirectory(baseDirectory);
            }

            if (File.Exists(filePath))
            {
                using (StreamWriter writer = File.AppendText(filePath))
                {
                    writer.WriteLine($"PCTR64 - Executed at {DateTime.Now}");
                }
            }
            else
            {
                // If file does not exist, create a new file and write to it
                using (StreamWriter writer = File.CreateText(filePath))
                {
                    writer.WriteLine($"PCTR64 - Executed at {DateTime.Now}");
                }
            }
        }

        public void ScheduleRecurringJob(string cronExpression, string jobName)
        {
            RecurringJob.AddOrUpdate(jobName, () => ExecuteJob(jobName), cronExpression);
        }

        [CanBePaused]
        public void ExecuteJob(string jobName)
        {
            HangFireLog newData = new HangFireLog
            {
                JobId = jobName,
                LogName = $"{_machineIdentifier}",
                LogTime = DateTime.Now,
            };
            _collection.InsertOne(newData);

            // To print in file
            WriteToTextFile();
        }

        public void StopRecurringJob(string jobName)
        {
            RecurringJob.RemoveIfExists(jobName);
        }


        public void StopJob(string JobId)
        {
            RecurringJob.RemoveIfExists(JobId);
        }
        public void PauseJob(string jobId)
        {
            PausedJobs.Add(jobId);
        }

        public void ResumeJob(string jobId)
        {
            PausedJobs.Remove(jobId);
        }
        public static bool IsJobPaused(string jobId)
        {
            lock (PausedJobs)
            {
                return PausedJobs.Contains(jobId);
            }
        }
    }
    public class CanBePausedAttribute : JobFilterAttribute, IServerFilter
    {
        public void OnPerforming(PerformingContext filterContext)
        {
            var jobId = filterContext.Job.Arguments[0].ToString().Trim('"');
            // Check if the job ID is in the paused jobs list
            if (JobScheduler.IsJobPaused(jobId))
            {
                filterContext.Canceled = true; // Skip execution if paused
            }
        }

        public void OnPerformed(PerformedContext filterContext)
        {
            // Logic after job execution (if needed)
        }
    }

    public class HangFireLog
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string LogName { get; set; }
        public DateTime LogTime { get; set; }
        public string JobId { get; set; }
    }
}
