using HaNoiTravel.DTOS;

namespace HaNoiTravel.Interfaces
{
    public interface ICustomerService
    {
        Task<int?> GetCustomerIdByUserIdAsync(int userId);
        Task<bool> ValidateAddressOwnershipAsync(int addressId, int customerId);
        Task<UserProfileDto?> GetUserProfileAsync(int userId);

        // Phương thức mới để cập nhật thông tin profile người dùng
        Task<bool> UpdateUserProfileAsync(int userId, UpdateUserProfileDto model);
    }
}
