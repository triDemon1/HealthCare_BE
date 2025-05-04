namespace HaNoiTravel.DTOS
{
    public class BookingResponse
    {
        public int BookingId { get; set; }
        public int ServiceId { get; set; }
        public string ServiceName { get; set; } // Tên dịch vụ
        public string CustomerName { get; set; }
        public int SubjectId { get; set; }
        public string? SubjectName { get; set; } // Tên đối tượng
        public int StatusId { get; set; }
        public string StatusName { get; set; } // Tên trạng thái booking
        public DateTime ScheduledStartTime { get; set; }
        public DateTime ScheduledEndTime { get; set; }
        public decimal PriceAtBooking { get; set; }
        public string? Notes { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
