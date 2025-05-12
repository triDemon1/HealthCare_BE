using HaNoiTravel.DTOS;
using HaNoiTravel.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        // [Authorize(Roles = "Admin")] // Apply authorization
        // Nhận DTO kết hợp từ form
        public async Task<ActionResult<Products>> CreateProduct([FromForm] CreateProductRequestDto productRequestDto) // Nhận DTO kết hợp
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // Kiểm tra validation từ DTO
            }

            try
            {
                // Gọi service method xử lý lưu file và tạo sản phẩm
                // Truyền toàn bộ DTO kết hợp vào service
                var createdProduct = await _productService.CreateProductAsync(productRequestDto);

                if (createdProduct == null)
                {
                    // Handle cases where creation might fail (e.g., validation in service)
                    // Có thể trả về lỗi cụ thể hơn từ service nếu cần
                    return BadRequest("Could not create product."); // Generic error, refine as needed
                }

                // Return 201 Created with the location of the new resource
                return CreatedAtAction(nameof(GetProduct), new { id = createdProduct.ProductId }, createdProduct);
            }
            catch (Exception ex)
            {
                // Log the exception (use a proper logging framework)
                Console.WriteLine($"Error creating product: {ex.Message}");
                // Return a generic 500 error
                return StatusCode(500, "An error occurred while creating the product.");
            }
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        // [Authorize(Roles = "Admin")] // Apply authorization
        // Nhận DTO kết hợp từ form
        public async Task<IActionResult> UpdateProduct(int id, [FromForm] UpdateProductRequestDto productRequestDto) // Nhận DTO kết hợp
        {
            // Kiểm tra ID trong URL có khớp với ID trong DTO không
            if (id != productRequestDto.ProductId)
            {
                return BadRequest("Product ID in URL does not match body.");
            }

            // Kiểm tra validation từ DTO
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // Gọi service method xử lý lưu file và cập nhật sản phẩm
                // Truyền toàn bộ DTO kết hợp vào service
                var updatedProduct = await _productService.UpdateProductAsync(id, productRequestDto);

                if (updatedProduct == null)
                {
                    return NotFound(); // Product not found (handled in service)
                }

                return Ok(updatedProduct); // Return the updated product DTO
            }
            catch (DbUpdateConcurrencyException)
            {
                // Handle concurrency conflict (e.g., product deleted by another user)
                if (!await _productService.ProductExistsAsync(id))
                {
                    return NotFound(); // Product was deleted concurrently
                }
                else
                {
                    throw; // Re-throw other concurrency exceptions
                }
            }
            catch (Exception ex)
            {
                // Log the exception (use a proper logging framework)
                Console.WriteLine($"Error updating product {id}: {ex.Message}");
                // Return a generic 500 error
                return StatusCode(500, "An error occurred while updating the product.");
            }
        }

        // DELETE: api/products/{id} (Delete a product)
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)] // 204 indicates successful deletion with no content
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        // [Authorize(Roles = "Admin")] // Apply authorization
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var deleted = await _productService.DeleteProductAsync(id);

            if (!deleted)
            {
                return NotFound(); // Product not found
            }

            return NoContent(); // Successful deletion
        }
    }
}
