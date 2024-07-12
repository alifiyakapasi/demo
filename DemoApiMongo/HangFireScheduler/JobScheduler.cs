using Hangfire;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace DemoApiMongo.HangFireScheduler
{
    public class JobScheduler
    {
        private readonly IMongoCollection<HangFireLog> _collection;
        private readonly string _machineIdentifier;

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
            RecurringJob.AddOrUpdate(jobName, () => ExecuteJob(cronExpression), cronExpression);
        }

        public void ExecuteJob(string cronExpression)
        {
            HangFireLog newData = new HangFireLog
            {
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
    }

    public class HangFireLog
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string LogName { get; set; }
        public DateTime LogTime { get; set; }
    }
}
