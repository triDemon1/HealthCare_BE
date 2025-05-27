using System.ComponentModel.DataAnnotations;

namespace HaNoiTravel.DTOS
{
    public class ResetPasswordRequest
    {
        [Required(ErrorMessage = "Email hoặc số điện thoại không được để trống.")]
        public string EmailOrPhone { get; set; } = string.Empty;

        // Thay thế Otp bằng TempResetToken
        [Required(ErrorMessage = "Token đặt lại mật khẩu không được để trống.")]
        public string TempResetToken { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu mới không được để trống.")]
        [MinLength(6, ErrorMessage = "Mật khẩu mới phải có ít nhất 6 ký tự.")]
        [StringLength(100, ErrorMessage = "Mật khẩu mới không được vượt quá 100 ký tự.")]
        public string NewPassword { get; set; } = string.Empty;
    }
}
