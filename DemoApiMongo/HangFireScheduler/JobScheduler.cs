using Hangfire;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using StackExchange.Redis;

namespace DemoApiMongo.HangFireScheduler
{
    public class JobScheduler
    {
        private readonly IMongoCollection<HangFireLog> _collection;
        private readonly string _machineIdentifier;

        public JobScheduler(IMongoDatabase database)
        {
            _collection = database.GetCollection<HangFireLog>("HangFireLog");
            _machineIdentifier = Environment.MachineName;
        }
        public void WriteToTextFile()
        {
            string filePath = "C:/Users/pctr64/Desktop/hangfire2.txt";

            using (StreamWriter writer = File.AppendText(filePath))
            {
                writer.WriteLine($"PCTR64 - Executed at {DateTime.Now}");
            }
        }

        public void ScheduleRecurringJob(string cronExpression)
        {
            string jobName = $"{_machineIdentifier}";
            //RecurringJob.AddOrUpdate<JobScheduler>(jobName, x => ExecuteJob(cronExpression), cronExpression);
            RecurringJob.AddOrUpdate(jobName, () => ExecuteJob(cronExpression), cronExpression);

            //RecurringJob.AddOrUpdate<JobScheduler>("HangFire Example", x => ExecuteJob(cronExpression), cronExpression);
        }
        public void ExecuteJob(string cronExpression)
        {
            HangFireLog newData = new HangFireLog
            {
                LogName = "PCTR64",
                LogTime = DateTime.Now,
            };
            _collection.InsertOne(newData);

            // To print in file
           // WriteToTextFile();
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
