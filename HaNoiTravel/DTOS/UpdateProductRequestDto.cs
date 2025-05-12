using System.ComponentModel.DataAnnotations;

namespace HaNoiTravel.DTOS
{
    public class UpdateProductRequestDto
    {
        [Required] // ProductId là bắt buộc khi cập nhật
        public int ProductId { get; set; }

        [Required]
        public int CategoryId { get; set; }

        [Required]
        [StringLength(255)]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal Price { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Stock Quantity must be non-negative")]
        public int StockQuantity { get; set; }

        public string? Sku { get; set; }

        [Required]
        public bool IsActive { get; set; }

        // Thuộc tính để nhận tệp hình ảnh từ form
        // Tên thuộc tính này ('ImageFile') phải khớp với tên bạn append vào FormData ở frontend
        public IFormFile? ImageFile { get; set; }
    }
}
