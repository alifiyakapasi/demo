using DemoApiMongo.HangFireScheduler;
using Microsoft.AspNetCore.Mvc;

namespace DemoApiMongo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuartzController : ControllerBase
    {
        private readonly QuartzManager _quartzManager;

        public QuartzController(QuartzManager quartzManager)
        {
            _quartzManager = quartzManager;
        }

        [HttpPost("start")]
        public async Task<ActionResult> StartJob()
        {
            await _quartzManager.StartAsync();
            return Ok("Job started successfully.");
        }

        [HttpPost("stop")]
        public async Task<ActionResult> StopJob()
        {
            await _quartzManager.StopAsync();
            return Ok("Job stopped successfully.");
        }

        [HttpGet("pause")]
        public async Task<IActionResult> PauseJob()
        {
            try
            {
                await _quartzManager.PauseJob();
                return Ok("Job paused successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Failed to pause job: {ex.Message}");
            }
        }

        [HttpGet("resume")]
        public async Task<IActionResult> ResumeJob()
        {
            try
            {
                await _quartzManager.ResumeJob();
                return Ok("Job resumed successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Failed to resume job: {ex.Message}");
            }
        }

    }
}
