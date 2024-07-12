using DemoApiMongo.Entities.ViewModels;
using DemoApiMongo.HangFireScheduler;
using DemoApiMongo.Repository;
using Hangfire.States;
using Hangfire;
using Microsoft.AspNetCore.Mvc;

namespace DemoApiMongo.Controllers
{
    [Route("api")]
    [ApiController]
    public class JobSchedulerController : Controller
    {
        private readonly JobScheduler _jobScheduler;

        public JobSchedulerController(JobScheduler jobScheduler)
        {
            _jobScheduler = jobScheduler;
        }

        [HttpPost("schedule-job")]
        public IActionResult ScheduleJob([FromBody] string cronExpression, string jobName)
        {
            _jobScheduler.ScheduleRecurringJob(cronExpression, jobName);

            return Ok("Recurring job scheduled with the provided cron expression.");
        }

        [HttpPost("stop-job")]
        public IActionResult StopJob(string jobName)
        {
            _jobScheduler.StopRecurringJob(jobName);

            return Ok("Recurring job is stopped.");
        }

        [HttpPost("hangfire-pause-job")]
        public IActionResult PauseJob(string jobId)
        {
            _jobScheduler.PauseJob(jobId);
            return Ok("Job paused successfully.");
        }

        [HttpPost("hangfire-resume-job")]
        public IActionResult ResumeJob(string jobId)
        {
            _jobScheduler.ResumeJob(jobId);
            return Ok("Job resumed successfully.");
        }
    }
}
