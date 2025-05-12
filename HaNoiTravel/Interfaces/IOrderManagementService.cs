using HaNoiTravel.DTOS;

namespace HaNoiTravel.Interfaces
{
    public interface IOrderManagementService
    {
        // Order Management
        Task<Pagination<OrderAdminDto>> GetAllOrdersAsync(int pageIndex, int pageSize);
        Task<OrderAdminDto?> GetOrderByIdAsync(int orderId);
        Task<OrderAdminDto?> UpdateOrderStatusAsync(int orderId, StatusUpdateDto statusDto); // Ví dụ cập nhật trạng thái
        //Task<bool> DeleteOrderAsync(int orderId);
    }
}
