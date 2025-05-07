namespace HaNoiTravel.DTOS
{
    public class CreatePaymentRequest
    {
        public int BookingId { get; set; }
        // Có thể thêm các thông tin khác nếu cần cho cổng thanh toán,
        // ví dụ: returnUrl, cancelUrl, paymentMethodType (e.g., 'ewallet', 'card')
        public string ReturnUrl { get; set; }
        public string CancelUrl { get; set; }
        // Giả định paymentMethodId sẽ được chọn ở frontend hoặc cấu hình cố định
        public int PaymentMethodId { get; set; }
    }
}
