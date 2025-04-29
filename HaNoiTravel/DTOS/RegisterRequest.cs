using System.ComponentModel.DataAnnotations;

namespace HaNoiTravel.DTOS
{
    public class RegisterRequest
    {
        // Thông tin cho bảng USER
        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string Username { get; set; }

        [Required]
        [StringLength(255, MinimumLength = 6)] // Độ dài tối thiểu cho mật khẩu (nên dài hơn trong thực tế)
        public string Password { get; set; }

        [Required]
        [EmailAddress] // Kiểm tra định dạng email
        [StringLength(255)]
        public string Email { get; set; }

        // PhoneNumber có thể null trong DB, nhưng thường required khi đăng ký
        [Phone] // Kiểm tra định dạng số điện thoại (tùy chọn, có thể custom validation)
        [StringLength(20)]
        public string PhoneNumber { get; set; }

        // Chúng ta sẽ gán ROLEID cố định là Customer (2) trong backend khi đăng ký user mới
        // Không cần nhận RoleId từ client

        // Thông tin cho bảng CUSTOMERS
        [Required]
        [StringLength(100)]
        public string FirstName { get; set; }

        [StringLength(100)]
        public string LastName { get; set; }

        // DateOfBirth và Gender có thể null trong DB, tùy bạn có bắt buộc nhập khi đăng ký không
        // Nếu bắt buộc, thêm [Required]
        public DateOnly? DateOfBirth { get; set; }
        public bool? Gender { get; set; } // true cho Nam, false cho Nữ, null nếu không cung cấp

        // Thông tin cho bảng ADDRESSES
        // Các trường địa chỉ có thể null trong DB, tùy bạn có bắt buộc nhập khi đăng ký không
        // Nếu bắt buộc, thêm [Required]
        [StringLength(100)]
        public string Country { get; set; }

        [StringLength(255)]
        public string Street { get; set; }

        [StringLength(100)]
        public string Ward { get; set; }

        [StringLength(100)]
        public string District { get; set; }

        [StringLength(100)]
        public string City { get; set; }
    }
}
