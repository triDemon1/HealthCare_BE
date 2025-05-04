using System.ComponentModel.DataAnnotations;

namespace HaNoiTravel.DTOS
{
    public class StatusUpdateDto
    {
        [Required]
        public int StatusId { get; set; }
        // Có thể thêm các trường khác nếu cần khi cập nhật trạng thái (ví dụ: ghi chú)
        public string? Notes { get; set; }
    }
}
