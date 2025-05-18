namespace HaNoiTravel.DTOS
{
    public class UserProfileDto
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty; // Có thể không cho phép sửa Username
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        // IsActive và RoleId thường không hiển thị cho người dùng cuối


        // Thông tin từ bảng CUSTOMERS (nếu người dùng có role Customer)
        public CustomerProfileDetailDto? Customer { get; set; }
    }
    public class CustomerProfileDetailDto
    {
        public int CustomerId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string? LastName { get; set; }
        public DateOnly? DateOfBirth { get; set; } // Sử dụng DateTime? thay vì DateOnly?
        public bool? Gender { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        // Thêm các trường khác của Customer nếu cần cho mục đích hiển thị
    }
}
