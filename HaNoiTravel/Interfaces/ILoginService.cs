using HaNoiTravel.DTOS;
using HaNoiTravel.Models;

namespace HaNoiTravel.Interfaces
{
    public interface ILoginService
    {
        // Phương thức xử lý logic đăng nhập
        // Trả về LoginResponse nếu thành công, null nếu thất bại (sai thông tin đăng nhập)
        Task<LoginResponse> LoginAsync(LoginRequest loginRequest, string ipAddress);

        // *** Thêm phương thức xử lý Refresh Token ***
        // Trả về Tuple<string, RefreshToken, string> (AccessToken mới, RefreshToken mới, Role)
        // hoặc null nếu Refresh Token không hợp lệ
        Task<Tuple<string, RefreshToken, string>> ValidateRefreshTokenAndGenerateNewTokensAsync(string refreshToken, string ipAddress);

        // *** Thêm phương thức thu hồi Refresh Token (cho Logout) ***
        // Trả về true nếu thu hồi thành công, false nếu không tìm thấy hoặc đã thu hồi
        Task<bool> RevokeRefreshTokenAsync(string refreshToken, string ipAddress, string reason = null);
    }
}
