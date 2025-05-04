namespace HaNoiTravel.DTOS
{
    public class usersAdminDto
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string? PhoneNumber { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsActive { get; set; }
        public int RoleId { get; set; }
        public string RoleName { get; set; } // Tên role
        public int? CustomerId { get; set; } // CustomerId nếu là Customer
        public int? StaffId { get; set; } // StaffId nếu là Staff
        public CustomerAdminDetailDto? Customer { get; set; }
        public StaffAdminDetailDto? Staff { get; set; }
    }
    // DTO cho thông tin chi tiết Customer (khi trả về cùng UserAdminDto)
    public class CustomerAdminDetailDto
    {
        public int CustomerId { get; set; }
        public string FirstName { get; set; }
        public string? LastName { get; set; }
        public DateOnly? DateOfBirth { get; set; }
        public bool? Gender { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        // Có thể thêm các trường khác nếu cần
    }

    // DTO cho thông tin chi tiết Staff (khi trả về cùng UserAdminDto)
    public class StaffAdminDetailDto
    {
        public int StaffId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Skills { get; set; }
        public int? ExpYear { get; set; }
        public bool? IsAvailable { get; set; }
        public DateTime? CreateAt { get; set; } // Note: Property name is CreateAt in your model
        public DateTime? UpdatedAt { get; set; }
        // Có thể thêm các trường khác nếu cần
    }
}
