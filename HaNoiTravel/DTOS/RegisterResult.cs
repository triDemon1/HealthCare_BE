namespace HaNoiTravel.DTOS
{
    public class RegisterResult
    {
        // Trạng thái đăng ký: true nếu thành công, false nếu thất bại
        public bool Success { get; set; }

        // Thông báo lỗi nếu đăng ký thất bại (null nếu thành công)
        public string ErrorMessage { get; set; }

        // Phương thức factory để tạo kết quả thành công
        public static RegisterResult SuccessResult()
        {
            return new RegisterResult { Success = true, ErrorMessage = null };
        }

        // Phương thức factory để tạo kết quả thất bại với thông báo lỗi
        public static RegisterResult Failure(string errorMessage)
        {
            return new RegisterResult { Success = false, ErrorMessage = errorMessage };
        }
    }
}
