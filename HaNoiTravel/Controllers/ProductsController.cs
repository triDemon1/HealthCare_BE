using HaNoiTravel.DTOS;
using HaNoiTravel.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HaNoiTravel.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService; // Inject Interface

        public ProductsController(IProductService productService) // Constructor Injection
        {
            _productService = productService;
        }
        // GET: api/products/{id}
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetProduct(int id)
        {
            var productDto = await _productService.GetActiveProductByIdAsync(id);
            if (productDto == null)
            {
                return NotFound(); // Xử lý kết quả null từ service
            }
            return Ok(productDto);
        }
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)] // Thêm 400 nếu có validate tham số
        public async Task<ActionResult<Pagination<Products>>> GetProducts(
            [FromQuery] int pageIndex = 0, // Default trang 0
            [FromQuery] int pageSize = 10, // Default 10 mục/trang
            [FromQuery] int? categoryId = null) // categoryId là optional, null = tất cả
        {
            // Basic validation cho tham số phân trang
            if (pageIndex < 0)
            {
                return BadRequest("PageIndex must be non-negative.");
            }
            if (pageSize <= 0)
            {
                return BadRequest("PageSize must be positive.");
            }


            // Gọi service với các tham số nhận được
            var pagedResult = await _productService.GetPagedActiveProductsAsync(pageIndex, pageSize, categoryId);

            // Trả về kết quả PagedResult
            return Ok(pagedResult);
        }
        // GET: api/products/category/{categoryId}
        [HttpGet("category/{categoryId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        // [ProducesResponseType(StatusCodes.Status404NotFound)] // Có thể không cần nếu service trả về list rỗng
        public async Task<IActionResult> GetProductsByCategory(int categoryId)
        {
            var (products, errorMessage) = await _productService.GetActiveProductsByCategoryIdAsync(categoryId);

            if (errorMessage != null)
            {
                return BadRequest(new { message = errorMessage }); // Trả về lỗi từ service
            }

            // Nếu không có lỗi, trả về danh sách (có thể rỗng)
            return Ok(products);
        }
    }
}
