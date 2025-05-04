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

        // Inject DbContext qua constructor
        public ProductService(AppDbContext context)
        {
            _context = context;
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
                return (null, $"Category with ID {categoryId} not found."); // Trả về lỗi
            }

            var products = await _context.Products
                                         .Where(p => p.Categoryid == categoryId && p.Isactive)
                                         .Select(p => MapToDto(p))
                                         .ToListAsync();
            return (products, null); // Trả về danh sách và không có lỗi
        }

        // Hàm helper MapToDto (có thể để private hoặc tạo một lớp Mapper riêng)
        private static Products MapToDto(Product product)
        {
            return new Products
            {
                ProductId = product.Productid,
                CategoryId = product.Categoryid,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                StockQuantity = product.Stockquantity,
                ImageUrl = product.Imageurl,
                Sku = product.Sku,
                IsActive = product.Isactive,
                CreatedAt = product.Createdat ?? DateTime.Now,
                UpdatedAt = product.Updatedat
            };
        }
        public async Task<Pagination<Products>> GetPagedActiveProductsAsync(int pageIndex, int pageSize, int? categoryId)
        {
            // Bắt đầu query IQueryable từ Entity
            var query = _context.Products.AsQueryable();

            // Áp dụng lọc theo trạng thái Active
            query = query.Where(p => p.Isactive);

            // Áp dụng lọc theo danh mục nếu categoryId có giá trị
            if (categoryId.HasValue)
            {
                query = query.Where(p => p.Categoryid == categoryId.Value);
            }

            // Đếm tổng số mục khớp với điều kiện lọc (quan trọng!)
            var totalCount = await query.CountAsync();

            // Áp dụng Sắp xếp (OrderBy) - BẮT BUỘC cho phân trang đúng
            // Nên sắp xếp theo khóa chính hoặc một cột có tính duy nhất/thứ tự xác định
            query = query.OrderBy(p => p.Productid); // Sắp xếp theo ProductId

            // Áp dụng Phân trang (Skip và Take)
            var items = await query
                .Skip(pageIndex * pageSize) // Bỏ qua số lượng mục của các trang trước
                .Take(pageSize)           // Lấy số lượng mục của trang hiện tại
                .Select(p => MapToDto(p)) // Chuyển Entity sang DTO
                .ToListAsync();

            // Trả về kết quả trong DTO PagedResult
            return new Pagination<Products>
            {
                Items = items,
                TotalCount = totalCount,
                PageIndex = pageIndex,
                PageSize = pageSize
            };
        }
    }
}
