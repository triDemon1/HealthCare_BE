using System.ComponentModel.DataAnnotations;

namespace HaNoiTravel.DTOS
{
    public class ForgotPasswordRequest
    {
        [Required(ErrorMessage = "Email hoặc số điện thoại không được để trống.")]
        [StringLength(255, ErrorMessage = "Email hoặc số điện thoại không được vượt quá 255 ký tự.")]
        // Thêm Regex để kiểm tra định dạng email hoặc số điện thoại nếu cần
        // [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,4}$|^\d{10}$", ErrorMessage = "Định dạng email hoặc số điện thoại không hợp lệ.")]
        public string EmailOrPhone { get; set; } = string.Empty;
    }
}
