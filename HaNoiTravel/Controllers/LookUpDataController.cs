using HaNoiTravel.DTOS;
using HaNoiTravel.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HaNoiTravel.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LookUpDataController : ControllerBase
    {
        private readonly ILookupDataService _lookupDataService;
        public LookUpDataController(ILookupDataService lookupDataService)
        {
            _lookupDataService = lookupDataService;
        }
        [HttpGet("admin/roles")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<RoleDto>>> GetAllRoles()
        {
            var roles = await _lookupDataService.GetAllRolesAsync();
            return Ok(roles);
        }

        [HttpGet("admin/bookingstatuses")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<BookingStatusDto>>> GetAllBookingStatuses()
        {
            var statuses = await _lookupDataService.GetAllBookingStatusesAsync();
            return Ok(statuses);
        }

        [HttpGet("admin/orderstatuses")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<OrderStatusDto>>> GetAllOrderStatus()
        {
            var statuses = await _lookupDataService.GetAllOrderStatusAsync();
            return Ok(statuses);
        }
    }
}
