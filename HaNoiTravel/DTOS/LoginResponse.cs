namespace HaNoiTravel.DTOS
{
    public class LoginResponse
    {
        public string AccessToken { get; set; }
        public string Role { get; set; } // Giả định Role là string (ví dụ: "Admin", "Customer", "Staff")
                                         // Nếu RoleId là int, bạn có thể trả về RoleId hoặc ánh xạ sang tên Role
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string RefreshToken {  get; set; }
        public int? CustomerId { get; set; }
        public int? AddressId { get; set; }
    }

}
