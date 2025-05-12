using HaNoiTravel.DTOS;
using HaNoiTravel.Interfaces;
using HaNoiTravel.Models;
using HaNoiTravel.Services;
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

        // GET /api/subjecttypes
        [HttpGet("paymentStatus")]
        public async Task<ActionResult<IEnumerable<Subjecttype>>> GetPaymentStatus()
        {
            var subjectTypes = await _bookingService.GetPaymentStatusesAsync();
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
        // Endpoint mới để hủy booking
        [HttpPut("cancel/{bookingId}")] // Sử dụng HttpPut vì đây là hành động cập nhật trạng thái
        public async Task<ActionResult> CancelBooking(int bookingId)
        {
            // Lấy CustomerId từ token hoặc claims (đảm bảo người dùng hiện tại là chủ sở hữu booking)
            var customerIdClaim = User.FindFirst(ClaimTypes.NameIdentifier); // Hoặc claim nào bạn dùng cho CustomerId
            if (customerIdClaim == null || !int.TryParse(customerIdClaim.Value, out int customerId))
            {
                return Unauthorized(new { message = "Không xác định được thông tin khách hàng." });
            }

            var success = await _bookingService.CancelBookingAsync(bookingId, customerId);

            if (success)
            {
                return Ok(new { message = $"Booking {bookingId} đã được hủy thành công." });
            }
            else
            {
                // Trả về 400 nếu booking không tồn tại, không thuộc về khách hàng hoặc không thể hủy
                return BadRequest(new { message = $"Không thể hủy booking {bookingId}. Booking không tồn tại, không thuộc về bạn hoặc đã ở trạng thái không thể hủy." });
            }
        }


        // Cập nhật endpoint GetCustomerBookings để nhận tham số tìm kiếm
        [HttpGet("customers/{customerId}/bookings")]
        public async Task<ActionResult<Pagination<BookingResponse>>> GetCustomerBookings(
            int customerId,
            [FromQuery] int pageIndex = 0,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null) // Thêm tham số tìm kiếm
        {
            // Tùy chọn: Kiểm tra customerId từ route có khớp với customerId từ token không
            var customerIdClaim = User.FindFirst(ClaimTypes.NameIdentifier); // Hoặc claim nào bạn dùng cho CustomerId
            if (customerIdClaim == null || !int.TryParse(customerIdClaim.Value, out int authenticatedCustomerId) || authenticatedCustomerId != customerId)
            {
                return Unauthorized(new { message = "Bạn không có quyền xem lịch sử đặt lịch của khách hàng này." });
            }


            var paginatedBookings = await _bookingService.GetCustomerBookingsAsync(customerId, pageIndex, pageSize, searchTerm); // Truyền searchTerm vào service

            if (paginatedBookings == null || !paginatedBookings.Items.Any())
            {
                // Vẫn trả về 200 OK với danh sách rỗng và tổng số 0 khi không tìm thấy kết quả
                return Ok(new Pagination<BookingResponse>
                {
                    Items = new List<BookingResponse>(), // hoặc dữ liệu thực tế
                    TotalCount = 0,  // số lượng total items
                    PageIndex = pageIndex,
                    PageSize = pageSize
                });
            }

            return Ok(paginatedBookings);
        }
        [HttpGet("bookings/{bookingId}")]
        public async Task<ActionResult<BookingAdminDto>> GetBookingById(int bookingId)
        {
            var booking = await _bookingService.GetBookingByIdAsync(bookingId);
            if (booking == null)
            {
                return NotFound();
            }
            return Ok(booking);
        }
    }
}
