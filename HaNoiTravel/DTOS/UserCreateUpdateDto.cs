using System.ComponentModel.DataAnnotations;

namespace HaNoiTravel.DTOS
{
    public class UserCreateUpdateDto
    {
        [Required]
        [MaxLength(100)]
        public string Username { get; set; }

        // Mật khẩu chỉ bắt buộc khi tạo mới hoặc nếu cần thay đổi
        [MaxLength(255)]
        public string? Password { get; set; }

        [Required]
        [MaxLength(255)]
        [EmailAddress]
        public string Email { get; set; }

        [MaxLength(20)]
        public string? PhoneNumber { get; set; }

        public bool IsActive { get; set; }

        [Required]
        public int RoleId { get; set; }

        // Thông tin bổ sung nếu tạo Customer hoặc Staff
        public CustomerCreateUpdateDto? CustomerData { get; set; }
        public StaffCreateUpdateDto? StaffData { get; set; }
    }
    // DTO cho dữ liệu Customer khi tạo/cập nhật User
    public class CustomerCreateUpdateDto
    {
        [MaxLength(100)]
        public string? FirstName { get; set; }

        [MaxLength(100)]
        public string? LastName { get; set; }

        public DateOnly? DateOfBirth { get; set; }

        public bool? Gender { get; set; }
    }

    // DTO cho dữ liệu Staff khi tạo/cập nhật User
    public class StaffCreateUpdateDto
    {
        [MaxLength(100)]
        public string FirstName { get; set; }

        [MaxLength(100)]
        public string LastName { get; set; }

        [MaxLength(20)]
        public string? PhoneNumber { get; set; } // Staff phone number (can be different from User phone)

        public string? Skills { get; set; }

        public int? ExpYear { get; set; }

        public bool? IsAvailable { get; set; }
    }
}
