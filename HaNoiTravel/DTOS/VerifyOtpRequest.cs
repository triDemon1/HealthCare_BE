using System.ComponentModel.DataAnnotations;

namespace HaNoiTravel.DTOS
{
    public class VerifyOtpRequest
    {
        [Required(ErrorMessage = "Email hoặc số điện thoại không được để trống.")]
        public string EmailOrPhone { get; set; } = string.Empty; // Cần email/phone để tìm user

        [Required(ErrorMessage = "Mã OTP không được để trống.")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "Mã OTP phải có 6 chữ số.")]
        [RegularExpression("^[0-9]{6}$", ErrorMessage = "Mã OTP phải là 6 chữ số.")]
        public string Otp { get; set; } = string.Empty;
    }
}
