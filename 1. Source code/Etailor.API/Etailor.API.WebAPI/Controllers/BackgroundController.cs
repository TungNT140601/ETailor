using Etailor.API.Service.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Etailor.API.WebAPI.Controllers
{
    [Route("api/background-manage")]
    [ApiController]
    public class BackgroundController : ControllerBase
    {
        private readonly IBackgroundService backgroundService;

        public BackgroundController(IBackgroundService backgroundService)
        {
            this.backgroundService = backgroundService;
        }

        [HttpPut("start")]
        public IActionResult StartSchedule(string? id)
        {
            try
            {
                backgroundService.StartSchedule(id);
                return Ok("Schedule Run Success");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPut("stop")]
        public IActionResult StopSchedule(string? id)
        {
            try
            {
                backgroundService.StopSchedule(id);
                return Ok("Schedule Stop Success");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
