namespace HaNoiTravel.DTOS
{
    public class CreateOrderDto
    {
        public int AddressId { get; set; } // Cần lấy địa chỉ từ frontend hoặc profile user
        public List<OrderItemDto> Items { get; set; } = new List<OrderItemDto>();
    }
    public class OrderItemDto
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
