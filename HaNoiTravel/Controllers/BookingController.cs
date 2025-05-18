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


        public BookingController(IBookingService bookingService)
        {
            _bookingService = bookingService;
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
            // BƯỚC 1: Lấy UserID một cách đáng tin cậy từ token (ClaimTypes.NameIdentifier)
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized(new { message = "Không xác định được thông tin người dùng từ token." });
            }

            // BƯỚC 2: Dùng UserID này để truy vấn CustomerId từ DB (đáng tin cậy)
            int customerId;
            try
            {
                customerId = await _bookingService.GetCustomerIdByUserId(userId);

                if (customerId <= 0)
                {
                    return Unauthorized(new { message = "Thông tin khách hàng liên kết không hợp lệ." });
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Lỗi khi lấy thông tin profile cho UserID {userId}: {ex}");
                return StatusCode(500, new { message = "Đã xảy ra lỗi khi tải thông tin người dùng. Vui lòng thử lại." });
            }

            // BƯỚC 3: Truyền CustomerId đã được xác minh vào service để thực hiện logic nghiệp vụ và xác thực quyền sở hữu
            try
            {
                var success = await _bookingService.CancelBookingAsync(bookingId, customerId);

                if (success)
                {
                    return Ok(new { message = $"Lịch đặt {bookingId} đã được hủy thành công." });
                }
                else
                {
                    return StatusCode(500, new { message = "Không thể hủy lịch đặt. Vui lòng kiểm tra lại trạng thái lịch đặt." });
                }
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message }); // Trả về 403 Forbidden
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Lỗi nội bộ khi hủy lịch đặt {bookingId} cho khách hàng {customerId}: {ex}");
                return StatusCode(500, new { message = "Đã xảy ra lỗi nội bộ khi hủy lịch đặt. Vui lòng liên hệ hỗ trợ." });
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
            //var customerIdClaim = User.FindFirst(ClaimTypes.NameIdentifier); // Hoặc claim nào bạn dùng cho CustomerId
            //if (customerIdClaim == null || !int.TryParse(customerIdClaim.Value, out int authenticatedCustomerId) || authenticatedCustomerId != customerId)
            //{
            //    return Unauthorized(new { message = "Bạn không có quyền xem lịch sử đặt lịch của khách hàng này." });
            //}


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
