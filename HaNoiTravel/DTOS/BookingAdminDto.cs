namespace HaNoiTravel.DTOS
{
    public class BookingAdminDto : BookingResponse
    {
        // Thêm các trường Admin có thể cần xem
        public int CustomerId { get; set; } // CustomerId liên quan
        public int? StaffId { get; set; } // Staff được assign
        public string? StaffName { get; set; } // Tên Staff

        // Thông tin Address liên quan
        public int AddressId { get; set; }
        public string? AddressStreet { get; set; }
        public string? AddressWard { get; set; }
        public string? AddressDistrict { get; set; }
        public string? AddressCity { get; set; }
        public string? AddressCountry { get; set; }
    }
}
