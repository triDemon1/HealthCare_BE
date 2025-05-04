namespace HaNoiTravel.DTOS
{
    public class OrderAdminDto
    {
        public int OrderId { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } // Tên Customer
        public int OrderStatusId { get; set; }
        public string OrderStatusName { get; set; } // Tên trạng thái Order
        public int AddressId { get; set; }
        public string? AddressStreet { get; set; } // Thông tin Address
        public string? AddressWard { get; set; }
        public string? AddressDistrict { get; set; }
        public string? AddressCity { get; set; }
        public string? AddressCountry { get; set; }
        public DateTime? OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public IEnumerable<OrderDetailDto> OrderDetails { get; set; }
    }
    // DTO cho OrderDetail nếu cần xem chi tiết Order
    public class OrderDetailDto
    {
        public int OrderDetailId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } // Tên sản phẩm
        public int Quantity { get; set; }
        public decimal PriceAtPurchase { get; set; }
    }
}
