namespace HaNoiTravel.DTOS
{
    public class PurchaseHistoryDto
    {
        public string ItemType { get; set; } // "Order" hoặc "Booking"
        public DateTime? TransactionDate { get; set; }
        public string ItemName { get; set; } // Tên sản phẩm hoặc dịch vụ
        public decimal Amount { get; set; }  // Số tiền
        public string Status { get; set; }   // PaymentStatus
    }
}
