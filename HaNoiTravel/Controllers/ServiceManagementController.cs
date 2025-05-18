using HaNoiTravel.DTOS;
using HaNoiTravel.Interfaces;
using HaNoiTravel.Models;
using HaNoiTravel.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HaNoiTravel.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize (Roles = "Admin")]
    public class ServiceManagementController : ControllerBase
    {
        private readonly IServiceManagementService _serviceManagement; // Your service/repository instance
        private readonly ILogger<ServiceManagementController> _logger;

        public ServiceManagementController(IServiceManagementService serviceManagement, ILogger<ServiceManagementController> logger)
        {
            _serviceManagement = serviceManagement;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<Pagination<Service>>> GetServices(
            [FromQuery] int pageIndex = 0,
            [FromQuery] int pageSize = 10)
        {
            var pagedServices = await _serviceManagement.GetAllServicesAsync(pageIndex, pageSize);
            return Ok(pagedServices);
        }
        [HttpGet("servicegroups")]
        public async Task<ActionResult<IEnumerable<Servicegroup>>> GetAllRoles()
        {
            var roles = await _serviceManagement.GetServicegroupAsync();
            return Ok(roles);
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<ServiceDto?>> GetService(int id)
        {
            try
            {
                var service = await _serviceManagement.GetServiceByIdAsync(id);
                if (service == null)
                {
                    return NotFound("Dịch vụ không tìm thấy.");
                }
                return Ok(service);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải thông tin dịch vụ với ID: {ServiceId}", id);
                return StatusCode(500, "Đã xảy ra lỗi khi tải thông tin dịch vụ. Vui lòng thử lại.");
            }
        }

        /// <summary>
        /// Thêm dịch vụ mới.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ServiceDto>> AddService([FromBody] ServiceCreateDto service)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var created = await _serviceManagement.AddServiceAsync(service);
            return CreatedAtAction(nameof(GetService), new { id = created.Serviceid }, created);
        }

        /// <summary>
        /// Cập nhật dịch vụ hiện có.
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateService(int id, [FromBody] ServiceUpdateDto service)
        {
            if (id != service.Serviceid) return BadRequest("ID không khớp.");
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var updated = await _serviceManagement.UpdateServiceAsync(service);
            if (!updated) return NotFound("Dịch vụ không tồn tại để cập nhật.");
            return NoContent();
        }

        /// <summary>
        /// Xóa dịch vụ theo ID.
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteService(int id)
        {
            try
            {
                // Repository sẽ thực hiện việc xóa.
                var deleted = await _serviceManagement.DeleteServiceAsync(id);
                if (!deleted)
                {
                    return NotFound("Dịch vụ không tìm thấy để xóa.");
                }
                return NoContent(); // Trả về 204 No Content cho thành công
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa dịch vụ với ID: {ServiceId}", id);
                return StatusCode(500, "Không thể xóa dịch vụ. Vui lòng thử lại.");
            }
        }
        [HttpGet("servicegroupsbysubjecttype")]
        public async Task<ActionResult<IEnumerable<Servicegroup>>> GetServiceGroupsBySubjectType(int subjectTypeId)
        {
            var serviceGroups = await _serviceManagement.GetServiceGroupsBySubjectTypeAsync(subjectTypeId);
            if (serviceGroups == null || !serviceGroups.Any())
            {
                return NotFound("No service groups found for the given subject type.");
            }
            return Ok(serviceGroups);
        }
    }
}
