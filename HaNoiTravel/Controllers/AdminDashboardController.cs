using HaNoiTravel.DTOS;
using HaNoiTravel.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HaNoiTravel.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminDashboardController : ControllerBase
    {
        private readonly IAdminDashboardService _dashboardService;

        public AdminDashboardController(IAdminDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet("stats")]
        public async Task<ActionResult<DashboardStatsDto>> GetDashboardStats()
        {
            var stats = await _dashboardService.GetDashboardStatsAsync();
            return Ok(stats);
        }
        [HttpGet("bookings-by-subject-type")]
        public async Task<ActionResult<List<BookingsBySubjectTypeDto>>> GetBookingsBySubjectType()
        {
            var data = await _dashboardService.GetBookingsBySubjectTypeAsync();
            return Ok(data);
        }
        [HttpGet("top-services")]
        public async Task<ActionResult<List<TopServiceDto>>> GetTopBookedServices([FromQuery] int top = 5)
        {
            var result = await _dashboardService.GetTopBookedServicesAsync(top);
            return Ok(result);
        }

    }
}
