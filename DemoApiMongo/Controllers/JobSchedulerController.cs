using DemoApiMongo.Entities.ViewModels;
using DemoApiMongo.HangFireScheduler;
using DemoApiMongo.Repository;
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
        public IActionResult ScheduleJob([FromBody] string cronExpression)
        {
            _jobScheduler.ScheduleRecurringJob(cronExpression);

            return Ok("Recurring job scheduled with the provided cron expression.");
        }

        [HttpPost("stop-job")]
        public IActionResult StopJob(string jobName)
        {
            _jobScheduler.StopRecurringJob(jobName);

            return Ok("Recurring job is stopped.");
        }
    }
}
