namespace HaNoiTravel.DTOS
{
    public class Products
    {
        public int ProductId { get; set; }
        public int CategoryId { get; set; }
        public string Name { get; set; } = string.Empty; // Khởi tạo giá trị mặc định
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public string? ImageUrl { get; set; }
        public string? Sku { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        // Bạn có thể thêm CategoryName nếu cần join bảng
        // public string CategoryName { get; set; } = string.Empty;
    }
}
