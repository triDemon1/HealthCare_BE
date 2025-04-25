using HaNoiTravel.DTOS;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HaNoiTravel.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class BookingController : ControllerBase
    {
        [HttpPost("bookings")]
        public IActionResult Booking(Booking inputDto)
        {
            return Ok(new { message = "Đặt lịch thành công!", inputDto.Name, inputDto.Note, inputDto.CareType, inputDto.Date, inputDto.Time });
        }
    }
}
