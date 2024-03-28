using Etailor.API.Service.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Etailor.API.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet("order-dashboard")]
        public IActionResult GetOrderDashboard(DateTime? date)
        {
            var result = _dashboardService.GetOrderDashboard(date);
            return Ok(result);
        }
    }
}
