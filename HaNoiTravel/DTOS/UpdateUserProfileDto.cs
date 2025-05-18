using System.ComponentModel.DataAnnotations;

namespace HaNoiTravel.DTOS
{
    public class UpdateUserProfileDto
    {
        // Các trường từ bảng USER mà người dùng cuối được phép cập nhật
        [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
        [MaxLength(255)] // Thêm MaxLength dựa trên schema DB
        public string? Email { get; set; } // Cho phép null nếu không bắt buộc

        [Phone(ErrorMessage = "Số điện thoại không hợp lệ.")]
        [MaxLength(20)] // Thêm MaxLength dựa trên schema DB
        public string? PhoneNumber { get; set; } // Cho phép null nếu không bắt buộc

        // Password không có ở đây vì làm riêng

        // Thông tin từ bảng CUSTOMERS mà người dùng cuối được phép cập nhật
        [Required(ErrorMessage = "Tên không được để trống.")] // FirstName trong CustomerData là bắt buộc theo schema
        [MaxLength(100)] // Thêm MaxLength dựa trên schema DB
        public string? FirstName { get; set; } // Có thể là Firstname của Customer

        [MaxLength(100)] // Thêm MaxLength dựa trên schema DB
        public string? LastName { get; set; } // Có thể là Lastname của Customer

        public DateOnly? DateOfBirth { get; set; } // Sử dụng DateTime?
        public bool? Gender { get; set; }
    }
}
