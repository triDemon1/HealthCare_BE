using HaNoiTravel.DTOS;
namespace HaNoiTravel.Interfaces
{
    public interface IProductService
    {
        Task<IEnumerable<Products>> GetActiveProductsAsync();
        Task<Products?> GetActiveProductByIdAsync(int id); // Trả về nullable DTO
        Task<(IEnumerable<Products>?, string?)> GetActiveProductsByCategoryIdAsync(int categoryId);
        Task<Pagination<Products>> GetPagedActiveProductsAsync(int pageIndex, int pageSize, int? categoryId);
    }
}
