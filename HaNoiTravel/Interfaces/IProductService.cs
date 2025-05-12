using HaNoiTravel.DTOS;
namespace HaNoiTravel.Interfaces
{
    public interface IProductService
    {
        Task<IEnumerable<Products>> GetActiveProductsAsync();
        Task<Products?> GetActiveProductByIdAsync(int id); // Trả về nullable DTO
        Task<(IEnumerable<Products>?, string?)> GetActiveProductsByCategoryIdAsync(int categoryId);
        Task<Pagination<Products>> GetPagedActiveProductsAsync(int pageIndex, int pageSize, int? categoryId);

        // New methods for managing products (assuming Products DTO is used for creation/update)
        Task<Products?> CreateProductAsync(CreateProductRequestDto productRequestDto);
        Task<Products?> UpdateProductAsync(int id, UpdateProductRequestDto productRequestDto);
        Task<bool> DeleteProductAsync(int id); // Returns true if deleted, false if not found
        Task<bool> ProductExistsAsync(int id); // Helper to check if product exists
    }
}
