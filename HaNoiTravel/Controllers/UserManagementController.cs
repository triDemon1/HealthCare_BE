using HaNoiTravel.Data;
using HaNoiTravel.DTOS;
using HaNoiTravel.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HaNoiTravel.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserManagementController : ControllerBase
    {
        private readonly IUserManagementService _userManagementService;
        private readonly AppDbContext _context; // Inject DbContext to perform security checks
        public UserManagementController(IUserManagementService userManagementService, AppDbContext context)
        {
            _userManagementService = userManagementService;
            _context = context;
        }
        //// User Management
        //[HttpGet("admin/users")]
        //[Authorize(Roles = "Admin")] // Chỉ cho phép Admin truy cập
        //public async Task<ActionResult<IEnumerable<usersAdminDto>>> GetAllUsers()
        //{
        //    var users = await _userManagementService.GetAllUsersAsync();
        //    return Ok(users);
        //}
        [HttpGet("admin/users")] // Segment route cho endpoint lấy danh sách users
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Roles = "Admin")] // Chỉ cho phép Admin truy cập
        public async Task<ActionResult<Pagination<usersAdminDto>>> GetPagedUsers(
        [FromQuery] int pageIndex = 0, // Mặc định trang 0
        [FromQuery] int pageSize = 10,
        [FromQuery] int? roleId = null) // Mặc định 10 mục/trang
        {
            // Validate tham số phân trang cơ bản
            if (pageIndex < 0 || pageSize <= 0)
            {
                return BadRequest("Invalid pagination parameters.");
            }

            // Gọi Service để lấy dữ liệu phân trang
            var pagedResult = await _userManagementService.GetPagedUsersAsync(pageIndex, pageSize, roleId);

            // Trả về kết quả phân trang
            return Ok(pagedResult);
        }

        [HttpGet("admin/users/{userId}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<usersAdminDto>> GetUserById(int userId)
        {
            var user = await _userManagementService.GetUserByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }
            return Ok(user);
        }

        [HttpPost("admin/users")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<usersAdminDto>> CreateUser([FromBody] UserCreateUpdateDto userDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var createdUser = await _userManagementService.CreateUserAsync(userDto);
                // Return 201 Created with the location of the new resource
                return CreatedAtAction(nameof(GetUserById), new { userId = createdUser.UserId }, createdUser);
            }
            catch (ArgumentException ex)
            {
                // Handle specific business logic errors (e.g., username/email already exists)
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                // Handle other potential errors
                return StatusCode(500, new { message = "An error occurred while creating the user.", error = ex.Message });
            }
        }

        [HttpPut("admin/users/{userId}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<usersAdminDto>> UpdateUser(int userId, [FromBody] UserCreateUpdateDto userDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var updatedUser = await _userManagementService.UpdateUserAsync(userId, userDto);
                if (updatedUser == null)
                {
                    return NotFound();
                }
                return Ok(updatedUser);
            }
            catch (ArgumentException ex)
            {
                // Handle specific business logic errors
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                // Handle other potential errors
                return StatusCode(500, new { message = "An error occurred while updating the user.", error = ex.Message });
            }
        }

        [HttpDelete("admin/users/{userId}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteUser(int userId)
        {
            var success = await _userManagementService.DeleteUserAsync(userId);
            if (!success)
            {
                return NotFound();
            }
            return NoContent(); // 204 No Content indicates successful deletion
        }
        [HttpGet("admin/staff")]
        public async Task<ActionResult<IEnumerable<StaffAdminDetailDto>>> GetAllStaff()
        {
            var staffList = await _userManagementService.GetAllStaffAsync();
            return Ok(staffList);
        }
    }
}
