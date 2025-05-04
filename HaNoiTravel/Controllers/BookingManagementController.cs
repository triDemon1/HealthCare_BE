using HaNoiTravel.Data;
using HaNoiTravel.DTOS;
using HaNoiTravel.Interfaces;
using HaNoiTravel.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HaNoiTravel.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingManagementController : ControllerBase
    {
        private readonly IBookingManagementService _bookingManagementService;
        public BookingManagementController(IBookingManagementService bookingManagementService)
        {
            _bookingManagementService = bookingManagementService;
        }
        [HttpGet("admin/bookings")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<BookingAdminDto>>> GetAllBookings()
        {
            var bookings = await _bookingManagementService.GetAllBookingsAsync();
            return Ok(bookings);
        }

        [HttpGet("admin/bookings/{bookingId}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<BookingAdminDto>> GetBookingById(int bookingId)
        {
            var booking = await _bookingManagementService.GetBookingByIdAsync(bookingId);
            if (booking == null)
            {
                return NotFound();
            }
            return Ok(booking);
        }
        // --- NEW: Admin Update Endpoints ---

        [HttpPut("admin/bookings/{bookingId}/status")]
        public async Task<ActionResult<BookingAdminDto>> UpdateBookingStatus(int bookingId, [FromBody] StatusUpdateDto statusDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var updatedBooking = await _bookingManagementService.UpdateBookingStatusAsync(bookingId, statusDto);
                if (updatedBooking == null)
                {
                    return NotFound(); // Booking not found
                }
                return Ok(updatedBooking);
            }
            catch (ArgumentException ex)
            {
                // Handle specific business logic errors (e.g., status not found)
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                // Handle other potential errors
                return StatusCode(500, new { message = "An error occurred while updating booking status.", error = ex.Message });
            }
        }
        [HttpPut("admin/bookings/{bookingId}")]
        public async Task<ActionResult<BookingAdminDto>> UpdateBooking(int bookingId, [FromBody] BookingUpdateDto bookingDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var updatedBooking = await _bookingManagementService.UpdateBookingAsync(bookingId, bookingDto);
                if (updatedBooking == null)
                {
                    return NotFound(); // Booking not found
                }
                return Ok(updatedBooking);
            }
            catch (ArgumentException ex)
            {
                // Handle specific business logic errors (e.g., invalid IDs, date/time issues)
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                // Handle other potential errors
                return StatusCode(500, new { message = "An error occurred while updating the booking.", error = ex.Message });
            }
        }
    }
}
