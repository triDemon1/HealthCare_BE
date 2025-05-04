using HaNoiTravel.DTOS;
using HaNoiTravel.Interfaces;
using HaNoiTravel.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HaNoiTravel.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderManagementController : ControllerBase
    {
        private readonly IOrderManagementService _orderManagementService;
        public OrderManagementController(IOrderManagementService orderManagementService)
        {
            _orderManagementService = orderManagementService;
        }
        [HttpGet("admin/orders")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<OrderAdminDto>>> GetAllOrders()
        {
            var orders = await _orderManagementService.GetAllOrdersAsync();
            return Ok(orders);
        }

        [HttpGet("admin/orders/{orderId}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<OrderAdminDto>> GetOrderById(int orderId)
        {
            var order = await _orderManagementService.GetOrderByIdAsync(orderId);
            if (order == null)
            {
                return NotFound();
            }
            return Ok(order);
        }
        [HttpPut("admin/orders/{orderId}/status")]
        public async Task<ActionResult<OrderAdminDto>> UpdateOrderStatus(int orderId, [FromBody] StatusUpdateDto statusDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var updatedOrder = await _orderManagementService.UpdateOrderStatusAsync(orderId, statusDto);
                if (updatedOrder == null)
                {
                    return NotFound(); // Order not found
                }
                return Ok(updatedOrder);
            }
            catch (ArgumentException ex)
            {
                // Handle specific business logic errors (e.g., status not found)
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                // Handle other potential errors
                return StatusCode(500, new { message = "An error occurred while updating order status.", error = ex.Message });
            }
        }
    }
}
