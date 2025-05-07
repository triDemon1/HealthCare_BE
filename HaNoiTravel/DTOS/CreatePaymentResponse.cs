namespace HaNoiTravel.DTOS
{
    public class CreatePaymentResponse
    {
        public bool Success { get; set; }
        public string PaymentUrl { get; set; } // URL để redirect người dùng đến cổng thanh toán
        public string TransactionRef { get; set; } // Tham chiếu giao dịch nội bộ (TransactionID)
        public string Message { get; set; }
        public string ErrorCode { get; set; }
    }
}
