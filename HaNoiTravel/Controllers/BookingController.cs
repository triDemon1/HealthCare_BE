using HaNoiTravel.DTOS;
using HaNoiTravel.Interfaces;
using HaNoiTravel.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HaNoiTravel.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class BookingController : ControllerBase
    {
        private readonly IBookingService _bookingService;


        public BookingController(IBookingService healthCareService)
        {
            _bookingService = healthCareService;
        }

        // GET /api/subjecttypes
        [HttpGet("subjecttypes")]
        public async Task<ActionResult<IEnumerable<Subjecttype>>> GetSubjectTypes()
        {
            var subjectTypes = await _bookingService.GetSubjectTypesAsync();
            return Ok(subjectTypes);
        }

        // GET /api/services
        // GET /api/services?subjectTypeId={subjectTypeId}
        [HttpGet("services")]
        public async Task<ActionResult<IEnumerable<Service>>> GetServices([FromQuery] int? subjectTypeId)
        {
            if (subjectTypeId.HasValue && subjectTypeId > 0)
            {
                var services = await _bookingService.GetServicesBySubjectTypeAsync(subjectTypeId.Value);
                return Ok(services);
            }
            else
            {
                var services = await _bookingService.GetAllServicesAsync();
                return Ok(services);
            }
        }

        // GET /api/customers/{customerId}/addresses
        [HttpGet("customers/{customerId}/addresses")]
        public async Task<ActionResult<IEnumerable<Address>>> GetCustomerAddresses(int customerId)
        {
            // You might want to add validation here to ensure the customerId is valid
            // and potentially that the requesting user has permission to view these addresses.
            var addresses = await _bookingService.GetCustomerAddressesAsync(customerId);

            if (addresses == null || !addresses.Any())
            {
                return NotFound($"Addresses for customer with ID {customerId} not found.");
            }

            return Ok(addresses);
        }

        // GET /api/customers/{customerId}/subjects?typeId={typeId}
        [HttpGet("customers/{customerId}/subjects")]
        public async Task<ActionResult<IEnumerable<Subject>>> GetCustomerSubjects(int customerId, [FromQuery] int typeId)
        {
            // You might want to add validation here for customerId and typeId
            // and potentially authorization checks.
            var subjects = await _bookingService.GetExistingSubjectsAsync(customerId, typeId);

            if (subjects == null || !subjects.Any())
            {
                // Return 200 OK with empty list if no subjects found, or 404 Not Found
                // depending on your desired API behavior. Returning 200 with empty list is common.
                return Ok(new List<Subject>());
            }

            return Ok(subjects);
        }


        // POST /api/bookings
        [HttpPost("bookings")]
        public async Task<ActionResult> CreateBooking([FromBody] BookingPayload payload)
        {
            // Add model validation
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // You should add more business logic validation here before calling the service,
            // e.g., check if CustomerId, AddressId, ServiceId exist, validate dates, etc.

            var success = await _bookingService.CreateBookingAsync(payload);

            if (success)
            {
                // Return 201 Created status code if successful
                // Optionally, return the created booking object or its ID
                return StatusCode(201, new { message = "Booking created successfully." });
            }
            else
            {
                // Return 400 Bad Request or 500 Internal Server Error depending on the failure reason
                return BadRequest(new { message = "Failed to create booking." });
            }
        }
        // --- NEW: Get bookings for a specific customer endpoint ---
        // GET /api/customers/{customerId}/bookings
        [HttpGet("customers/{customerId}/bookings")]
        public async Task<ActionResult<IEnumerable<BookingResponse>>> GetCustomerBookings(int customerId)
        {
            var bookings = await _bookingService.GetCustomerBookingsAsync(customerId);

            if (bookings == null || !bookings.Any())
            {
                // Return 200 OK with an empty list if no bookings are found for this customer.
                return Ok(new List<BookingResponse>());
            }

            return Ok(bookings);
        }
    }
}
