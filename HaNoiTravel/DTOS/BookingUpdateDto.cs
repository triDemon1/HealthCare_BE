using System.ComponentModel.DataAnnotations;

namespace HaNoiTravel.DTOS
{
    public class BookingUpdateDto
    {
        [Required]
        public int ServiceId { get; set; } // Dịch vụ mới
        public int? StaffId { get; set; } // Nhân viên mới (có thể null)

        [Required]
        public DateTime ScheduledStartTime { get; set; } // Thời gian bắt đầu mới

        [Required]
        public DateTime ScheduledEndTime { get; set; } // Thời gian kết thúc mới

        [Required]
        [Range(0, double.MaxValue)] // Giá không âm
        public decimal PriceAtBooking { get; set; } // Giá mới

        [Required]
        public int AddressId { get; set; } // Địa chỉ mới

        [Required]
        public int StatusId { get; set; } // Trạng thái mới (vẫn cần cập nhật trạng thái)

        public string? Notes { get; set; } // Ghi chú (có thể null)
        public int? PaymentStatusId { get; set; }
    }
}
