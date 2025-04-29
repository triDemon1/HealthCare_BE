using HaNoiTravel.DTOS;
namespace HaNoiTravel.Interfaces
{
    public interface IRegisterService
    {
        // *** Thêm phương thức xử lý logic đăng ký ***
        // Trả về true nếu đăng ký thành công, false nếu có lỗi (ví dụ: trùng lặp)
        // Có thể trả về một enum hoặc custom class để chỉ rõ loại lỗi
        Task<RegisterResult> RegisterAsync(RegisterRequest registerRequest);
    }
}
