using HaNoiTravel.DTOS;
using HaNoiTravel.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HaNoiTravel.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService; // Inject Interface

        public OrdersController(IOrderService orderService) // Constructor Injection
        {
            _orderService = orderService;
        }

        // POST: api/orders/checkout
        [HttpPost("checkout")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Checkout([FromBody] CreateOrderDto createOrderDto)
        {
            // Lấy ClaimsPrincipal từ HttpContext.User
            var user = HttpContext.User;

            // Gọi service để xử lý logic
            var (orderId, errorMessage) = await _orderService.CreateOrderAsync(createOrderDto, user);

            if (errorMessage != null)
            {
                // Phân loại lỗi dựa trên nội dung errorMessage nếu cần để trả về status code phù hợp hơn
                if (errorMessage.Contains("User ID not found") || errorMessage.Contains("unauthorized address"))
                    return Unauthorized(new { message = errorMessage });
                if (errorMessage.Contains("not found") || errorMessage.Contains("Insufficient stock") || errorMessage.Contains("Invalid"))
                    return BadRequest(new { message = errorMessage }); // Các lỗi do dữ liệu đầu vào

                // Lỗi server nội bộ
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = errorMessage });
            }

            // Thành công
            return Ok(new { OrderId = orderId, Message = "Order created successfully." });
        }
    }
}
