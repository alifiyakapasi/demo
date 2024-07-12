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
    private HelloJob _helloJob;
    private JobKey _jobKey;

    public QuartzManager()
    {
        _helloJob = new HelloJob();
    }

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
       .WithCronSchedule("0/15 * * 1/1 * ? *")
       .Build();
        _jobKey = job.Key;
        await _scheduler.ScheduleJob(job, trigger);
    }

    public async Task PauseJob()
    {
        var jobKey = new JobKey("PCTR64job1", "PCTR64group1");
        await _scheduler.PauseJob(jobKey);
        await _helloJob.UpdatePauseTime(_jobKey);
    }

    public async Task ResumeJob()
    {
        var jobKey = new JobKey("PCTR64job1", "PCTR64group1");
        await _helloJob.UpdateResumeTime(_jobKey);
        await _scheduler.ResumeJob(jobKey);
    }

    public async Task StopAsync()
    {
        await _scheduler?.Shutdown();
    }
}

public class HelloJob : IJob
{
    const string connectionUri = "your_connection_string";
    const string dbName = "DEV";
    MongoClient client = new MongoClient(connectionUri);
    private readonly IMongoCollection<QuartzLog> _collection;
    private readonly IMongoCollection<QuartzLog> _firstdatacollection;

    private readonly string _machineIdentifier;


    public HelloJob()
    {
        _machineIdentifier = Environment.MachineName;

        try
        {
            _collection = client.GetDatabase(dbName).GetCollection<QuartzLog>("QuartzLog");
            _firstdatacollection = client.GetDatabase(dbName).GetCollection<QuartzLog>("FirstQuartzLog");
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    public async Task Execute(IJobExecutionContext context)
    {
        if (IsFirstExecution(context.JobDetail.Key))
        {
            Console.WriteLine("Hello, Quartz.NET!" + DateTime.Now);

            QuartzLog newData = new QuartzLog
            {
                LogName = $"{_machineIdentifier}",
                LogTime = DateTime.Now,
                JobKey = context.JobDetail.Key,
            };
            try
            {
                await _firstdatacollection.InsertOneAsync(newData);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message}");
            }
            await Task.CompletedTask;
        }
    }

    private bool IsFirstExecution(JobKey jobKey)
    {
        var filter = Builders<QuartzLog>.Filter.Eq(x => x.JobKey, jobKey);
        var count = _collection.Find(filter).CountDocuments();

        QuartzLog firstdata = new QuartzLog
        {
            LogName = $"{_machineIdentifier}",
            LogTime = DateTime.Now,
            JobKey = jobKey,
        };
        _collection.InsertOneAsync(firstdata);
        return count == 0;
    }

    public async Task UpdatePauseTime(JobKey jobKey)
    {
        var filter = Builders<QuartzLog>.Filter.Eq(x => x.JobKey, jobKey);
        var combinedUpdate = Builders<QuartzLog>.Update
         .Combine(
             Builders<QuartzLog>.Update.Set(x => x.PauseTime, DateTime.Now)
         );

        await _firstdatacollection.UpdateManyAsync(filter, combinedUpdate);
    }

    public async Task UpdateResumeTime(JobKey jobKey)
    {
        var filter = Builders<QuartzLog>.Filter.Eq(x => x.JobKey, jobKey);
        var combinedUpdate = Builders<QuartzLog>.Update
         .Combine(
             Builders<QuartzLog>.Update.Set(x => x.ResumeTime, DateTime.Now)
         );

        await _firstdatacollection.UpdateManyAsync(filter, combinedUpdate);
    }
}


public class QuartzLog
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }
    public string LogName { get; set; }
    public DateTime LogTime { get; set; }
    public DateTime? PauseTime { get; set; } 
    public DateTime? ResumeTime { get; set; }
    public JobKey JobKey { get; set; }
}