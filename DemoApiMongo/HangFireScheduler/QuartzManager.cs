namespace DemoApiMongo.HangFireScheduler;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using Quartz;
using Quartz.Impl;
using System;
using System.Threading.Tasks;

public class QuartzManager
{
    private IScheduler _scheduler;

    public async Task StartAsync()
    {

        // Grab the Scheduler instance from the Factory
        _scheduler = await StdSchedulerFactory.GetDefaultScheduler();
        await _scheduler.Start();

        // Define the job and tie it to our HelloJob class
        IJobDetail job = JobBuilder.Create<HelloJob>()
            .WithIdentity("PCTR64job1", "PCTR64group1")
            .Build();

        // Trigger the job to run now, and then repeat every 10 seconds
        //ITrigger trigger = TriggerBuilder.Create()
        //    .WithIdentity("trigger1", "group1")
        //    .StartNow()
        //    .WithSimpleSchedule(x => x
        //        .WithIntervalInMinutes(2)
        //        .RepeatForever())
        //    .Build();

        ITrigger trigger = TriggerBuilder.Create()
       .WithIdentity("PCTR64trigger1", "PCTR64group1")
       .StartNow()
       .WithCronSchedule("0 0/2 * 1/1 * ? *")
       .Build();

        await _scheduler.ScheduleJob(job, trigger);
    }


    public async Task StopAsync()
    {
        await _scheduler?.Shutdown();
    }
}

public class HelloJob : IJob
{
    const string connectionUri = "your connection string";
    const string dbName = "DEV";

    MongoClient client = new MongoClient(connectionUri);

    private readonly IMongoCollection<QuartzLog> _collection;

    public HelloJob()
    {
        try
        {
            _collection = client.GetDatabase(dbName).GetCollection<QuartzLog>("QuartzLog");
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    public async Task Execute(IJobExecutionContext context)
    {
        Console.WriteLine("Hello, Quartz.NET!" + DateTime.Now);

        QuartzLog newData = new QuartzLog
        {
            LogName = "PCTR64",
            LogTime = DateTime.Now,
        };
        try
        {
            await _collection.InsertOneAsync(newData);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{ex.Message}");
        }
        await Task.CompletedTask;
    }
}

public class QuartzLog
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }
    public string LogName { get; set; }
    public DateTime LogTime { get; set; }
}