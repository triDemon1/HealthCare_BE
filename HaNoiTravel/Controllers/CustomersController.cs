using HaNoiTravel.DTOS;
using HaNoiTravel.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HaNoiTravel.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CustomersController : ControllerBase
    {
        private readonly ICustomerService _customerService;


        public CustomersController(ICustomerService customerService)
        {
            _customerService = customerService;
        }
        [HttpGet("profile")]
        public async Task<ActionResult<UserProfileDto>> GetUserProfile() // Sử dụng UserProfileDto mới
        {
            // Lấy UserID từ claims của người dùng đã xác thực
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier); // Hoặc claim nào bạn dùng cho UserID
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized(new { message = "Không xác định được thông tin người dùng." });
            }

            var userProfile = await _customerService.GetUserProfileAsync(userId); // Gọi phương thức đã sửa

            if (userProfile == null)
            {
                // Trường hợp không tìm thấy User hoặc Customer profile
                return NotFound(new { message = "Không tìm thấy thông tin profile." });
            }

            return Ok(userProfile);
        }

        // PUT: api/User/profile
        [HttpPut("profile")]
        public async Task<ActionResult> UpdateUserProfile([FromBody] UpdateUserProfileDto model) // Sử dụng UpdateUserProfileDto mới
        {
            // Lấy UserID từ claims của người dùng đã xác thực
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier); // Hoặc claim nào bạn dùng cho UserID
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized(new { message = "Không xác định được thông tin người dùng." });
            }

            // Kiểm tra Model State Validation
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var success = await _customerService.UpdateUserProfileAsync(userId, model); // Gọi phương thức đã sửa

                if (success)
                {
                    return Ok(new { message = "Cập nhật profile thành công." });
                }
                else
                {
                    // Trường hợp Service trả về false (ví dụ: không có thay đổi)
                    return BadRequest(new { message = "Cập nhật profile thất bại hoặc không có thay đổi." });
                }
            }
            catch (InvalidOperationException ex)
            {
                // Bắt các exception nghiệp vụ từ Service
                return BadRequest(new { message = ex.Message }); // Trả về thông báo lỗi chi tiết
            }
            catch (Exception ex)
            {
                // Bắt các exception khác (ví dụ: lỗi DB)
                Console.Error.WriteLine($"Error updating user profile for user {userId}: {ex}");
                return StatusCode(500, new { message = "Đã xảy ra lỗi nội bộ khi cập nhật profile." });
            }
        }
    }
}
