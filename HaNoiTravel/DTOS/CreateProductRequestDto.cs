using System.ComponentModel.DataAnnotations;

namespace HaNoiTravel.DTOS
{
    public class CreateProductRequestDto
    {
        [Required] // Ví dụ validation: trường này là bắt buộc
        public int CategoryId { get; set; }

        [Required]
        [StringLength(255)] // Ví dụ validation: giới hạn độ dài chuỗi
        public string Name { get; set; } = string.Empty; // Khởi tạo mặc định để tránh null

        public string? Description { get; set; } // Có thể null

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")] // Ví dụ validation
        public decimal Price { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Stock Quantity must be non-negative")] // Ví dụ validation
        public int StockQuantity { get; set; }

        public string? Sku { get; set; } // Có thể null

        [Required]
        public bool IsActive { get; set; }

        // Thuộc tính để nhận tệp hình ảnh từ form
        // Tên thuộc tính này ('ImageFile') phải khớp với tên bạn append vào FormData ở frontend
        public IFormFile? ImageFile { get; set; }
    }
}
