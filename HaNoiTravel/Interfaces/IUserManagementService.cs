
using HaNoiTravel.DTOS;

namespace HaNoiTravel.Interfaces
{
    public interface IUserManagementService
    {
        // User Management
        //Task<IEnumerable<usersAdminDto>> GetAllUsersAsync();
        Task<usersAdminDto?> GetUserByIdAsync(int userId);
        Task<usersAdminDto> CreateUserAsync(UserCreateUpdateDto userDto); // Trả về DTO của user vừa tạo
        Task<usersAdminDto?> UpdateUserAsync(int userId, UserCreateUpdateDto userDto); // Trả về DTO của user vừa cập nhật
        Task<bool> DeleteUserAsync(int userId); // Trả về true nếu xóa thành công
        Task<IEnumerable<StaffAdminDetailDto>> GetAllStaffAsync();
        Task<Pagination<usersAdminDto>> GetPagedUsersAsync(int pageIndex, int pageSize, int? RoleId);
    }
}
