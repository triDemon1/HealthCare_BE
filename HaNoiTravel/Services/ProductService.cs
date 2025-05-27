using HaNoiTravel.Data;
using HaNoiTravel.Interfaces;
using HaNoiTravel.Models;
using HaNoiTravel.DTOS;
using Microsoft.EntityFrameworkCore;

namespace HaNoiTravel.Services
{
    public class ProductService: IProductService
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly IHttpContextAccessor _httpContextAccessor;

        // Inject DbContext qua constructor
        public ProductService(AppDbContext context, IWebHostEnvironment hostingEnvironment, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _hostingEnvironment = hostingEnvironment;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IEnumerable<Products>> GetActiveProductsAsync()
        {
            return await _context.Products
                                 .Where(p => p.Isactive)
                                 .Select(p => MapToDto(p))
                                 .ToListAsync();
        }

        public async Task<Products?> GetActiveProductByIdAsync(int id)
        {
            var product = await _context.Products
                                        .AsNoTracking() // Tăng hiệu năng nếu chỉ đọc
                                        .FirstOrDefaultAsync(p => p.Productid == id && p.Isactive);

            return product == null ? null : MapToDto(product);
        }

        public async Task<(IEnumerable<Products>?, string?)> GetActiveProductsByCategoryIdAsync(int categoryId)
        {
            var categoryExists = await _context.Categories.AnyAsync(c => c.Categoryid == categoryId);
            if (!categoryExists)
            {
                return (null, $"Category with ID {categoryId} not found.");
            }

            // Bước 1: Lấy các thực thể Product từ database vào bộ nhớ trước
            var entities = await _context.Products
                                         .Where(p => p.Categoryid == categoryId && p.Isactive)
                                         .ToListAsync();

            // Bước 2: Sau đó, ánh xạ các thực thể trong bộ nhớ sang DTO và xây dựng URL đầy đủ
            var products = entities.Select(p => MapToDto(p)).ToList();
            return (products, null);
        }

        // Hàm helper MapToDto (có thể để private hoặc tạo một lớp Mapper riêng)
        private Products MapToDto(Product product) // Bỏ từ khóa 'static'
        {
            string? fullImageUrl = null;
            if (!string.IsNullOrEmpty(product.Imageurl))
            {
                var request = _httpContextAccessor.HttpContext?.Request;
                if (request != null)
                {
                    // Xây dựng URL gốc của backend (ví dụ: https://localhost:5000)
                    var baseUrl = $"{request.Scheme}://{request.Host}";
                    // Ghép URL gốc với đường dẫn tương đối từ DB
                    fullImageUrl = $"{baseUrl}{product.Imageurl}";
                }
                else
                {
                    // Trường hợp HttpContext không có (ví dụ: chạy background service), giữ nguyên đường dẫn tương đối
                    fullImageUrl = product.Imageurl;
                }
            }

            return new Products
            {
                ProductId = product.Productid,
                CategoryId = product.Categoryid,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                StockQuantity = product.Stockquantity,
                ImageUrl = fullImageUrl, // Sử dụng URL đầy đủ
                Sku = product.Sku,
                IsActive = product.Isactive,
                CreatedAt = product.Createdat ?? DateTime.Now,
                UpdatedAt = product.Updatedat
            };
        }
        public async Task<Pagination<Products>> GetPagedActiveProductsAsync(int pageIndex, int pageSize, int? categoryId)
        {
            var query = _context.Products.AsQueryable();
            query = query.Where(p => p.Isactive);

            if (categoryId.HasValue)
            {
                query = query.Where(p => p.Categoryid == categoryId.Value);
            }

            var totalCount = await query.CountAsync();
            query = query.OrderBy(p => p.Productid);

            // Bước 1: Lấy các thực thể Product đã được phân trang từ database vào bộ nhớ trước
            var entities = await query
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Bước 2: Sau đó, ánh xạ các thực thể trong bộ nhớ sang DTO và xây dựng URL đầy đủ
            var items = entities.Select(p => MapToDto(p)).ToList();

            return new Pagination<Products>
            {
                Items = items,
                TotalCount = totalCount,
                PageIndex = pageIndex,
                PageSize = pageSize
            };
        }
        public async Task<Products?> CreateProductAsync(CreateProductRequestDto productRequestDto) // Use the new combined DTO
        {
            string? imageUrl = null;
            // Handle file upload if a new file is provided in the DTO
            if (productRequestDto.ImageFile != null && productRequestDto.ImageFile.Length > 0)
            {
                // --- Add null check for WebRootPath ---
                if (_hostingEnvironment.WebRootPath == null)
                {
                    // Handle the error: Web root path is not available
                    // You might log this, throw a specific exception, or return an error message
                    Console.Error.WriteLine("WebRootPath is null. Cannot save file.");
                    // Depending on your application's needs, you might return null,
                    // throw a custom exception, or return a specific error DTO.
                    // For now, we'll throw an exception to indicate the failure.
                    throw new InvalidOperationException("Web root path is not configured. Cannot save file.");
                }
                // --------------------------------------

                // Define the path to save (e.g., wwwroot/assets/images/products)
                var uploadsFolder = Path.Combine(_hostingEnvironment.WebRootPath, "assets", "images", "products");
                // Create directory if it doesn't exist
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // Create a unique file name
                // Using Path.GetExtension to preserve the original file extension
                var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(productRequestDto.ImageFile.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                // Save the file
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await productRequestDto.ImageFile.CopyToAsync(fileStream);
                }

                // Generate the URL path to save in the database
                // Use forward slashes for URL paths
                imageUrl = $"/assets/images/products/{uniqueFileName}".Replace("\\", "/");
            }

            // Map DTO to Entity
            var product = new Product
            {
                // ProductId will be generated by the database
                Categoryid = productRequestDto.CategoryId,
                Name = productRequestDto.Name,
                Description = productRequestDto.Description,
                Price = productRequestDto.Price,
                Stockquantity = productRequestDto.StockQuantity,
                Imageurl = imageUrl, // Save the generated URL (can be null)
                Sku = productRequestDto.Sku,
                Isactive = productRequestDto.IsActive,
                Createdat = DateTime.Now,
                Updatedat = DateTime.Now // Set UpdatedAt on creation as well
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            // Map the saved entity back to DTO to return (includes generated ID and ImageUrl)
            return MapToDto(product);
        }

        // Updated method to update a product with file upload using combined DTO
        public async Task<Products?> UpdateProductAsync(int id, UpdateProductRequestDto productRequestDto) // Use the new combined DTO
        {
            var product = await _context.Products.FirstOrDefaultAsync(p => p.Productid == id);

            if (product == null)
            {
                return null; // Product not found
            }

            string? oldImageUrl = product.Imageurl; // Store old URL before updating

            // Handle file upload if a new file is provided in the DTO
            if (productRequestDto.ImageFile != null && productRequestDto.ImageFile.Length > 0)
            {
                // --- Add null check for WebRootPath ---
                if (_hostingEnvironment.WebRootPath == null)
                {
                    // Handle the error: Web root path is not available
                    Console.Error.WriteLine("WebRootPath is null. Cannot save file.");
                    throw new InvalidOperationException("Web root path is not configured. Cannot save file.");
                }
                // --------------------------------------

                // Define the path to save (e.g., wwwroot/assets/images/products)
                var uploadsFolder = Path.Combine(_hostingEnvironment.WebRootPath, "assets", "images", "products");
                // Create directory if it doesn't exist
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // Create a unique file name
                var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(productRequestDto.ImageFile.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                // Save the new file
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await productRequestDto.ImageFile.CopyToAsync(fileStream);
                }

                // Generate the new URL path
                string newImageUrl = $"/assets/images/products/{uniqueFileName}".Replace("\\", "/");

                // Update the product's ImageUrl
                product.Imageurl = newImageUrl;

                // Optional: Delete the old file if it exists and is not the same as the new one
                if (!string.IsNullOrEmpty(oldImageUrl) && oldImageUrl != newImageUrl)
                {
                    DeleteFile(oldImageUrl); // Call helper method to delete the old file
                }
            }
            // If no new file is provided in the DTO, the product.ImageUrl remains unchanged,
            // which means the old image URL is kept.

            // Update other entity properties from DTO
            product.Categoryid = productRequestDto.CategoryId;
            product.Name = productRequestDto.Name;
            product.Description = productRequestDto.Description;
            product.Price = productRequestDto.Price;
            product.Stockquantity = productRequestDto.StockQuantity;
            product.Sku = productRequestDto.Sku;
            product.Isactive = productRequestDto.IsActive; // Assuming IsActive is managed via DTO
            product.Updatedat = DateTime.Now; // Update UpdatedAt

            _context.Entry(product).State = EntityState.Modified; // Mark as modified
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await ProductExistsAsync(id)) // Check if product still exists
                {
                    return null; // Product was deleted by another process
                }
                else
                {
                    throw; // Re-throw other concurrency issues
                }
            }

            // Map the updated entity back to DTO to return
            return MapToDto(product);
        }

        private void DeleteFile(string fileUrl)
        {
            // Convert URL path to physical file path
            // Remove leading '/' if present
            var relativePath = fileUrl.TrimStart('/');
            var filePath = Path.Combine(_hostingEnvironment.WebRootPath, relativePath);

            // Check if file exists before attempting to delete
            if (File.Exists(filePath))
            {
                try
                {
                    File.Delete(filePath);
                    Console.WriteLine($"Deleted file: {filePath}"); // Log deletion
                }
                catch (Exception ex)
                {
                    // Log any errors during file deletion
                    Console.Error.WriteLine($"Error deleting file {filePath}: {ex.Message}");
                    // Consider how to handle deletion errors (e.g., retry, log and ignore)
                }
            }
        }

        // New method to delete a product
        public async Task<bool> DeleteProductAsync(int id)
        {
            var product = await _context.Products.FirstOrDefaultAsync(p => p.Productid == id);

            if (product == null)
            {
                return false; // Product not found
            }

            _context.Products.Remove(product); // Remove the entity
            await _context.SaveChangesAsync();

            return true; // Product deleted successfully
        }

        // Helper to check if product exists
        public async Task<bool> ProductExistsAsync(int id)
        {
            return await _context.Products.AnyAsync(e => e.Productid == id);
        }


        // Hàm helper MapToDto (có thể để private hoặc tạo một lớp Mapper riêng)
        // Make it public if needed elsewhere, otherwise keep private
        private static Product MapToEntity(Products productDto)
        {
            return new Product
            {
                Productid = productDto.ProductId, // Include ID if mapping for update
                Categoryid = productDto.CategoryId,
                Name = productDto.Name,
                Description = productDto.Description,
                Price = productDto.Price,
                Stockquantity = productDto.StockQuantity,
                Imageurl = productDto.ImageUrl,
                Sku = productDto.Sku,
                Isactive = productDto.IsActive,
                Createdat = productDto.CreatedAt, // Include if mapping existing data
                Updatedat = productDto.UpdatedAt
            };
        }
    }
}
