
using HaNoiTravel.DTOS;
using HaNoiTravel.Models;

namespace HaNoiTravel.Interfaces
{
    public interface IPaymentService
    {
        /// <summary>
        /// Khởi tạo yêu cầu thanh toán cho một Booking.
        /// </summary>
        /// <param name="request">Thông tin yêu cầu thanh toán.</param>
        /// <param name="customerId">ID của khách hàng thực hiện yêu cầu (để xác minh quyền).</param>
        /// <returns>Thông tin phản hồi bao gồm URL thanh toán.</returns>
        Task<CreatePaymentResponse> CreatePaymentForBookingAsync(CreatePaymentRequest request, int customerId);
        Task<CreatePaymentResponse> CreatePaymentForOrderAsync(CreatePaymentForOrderRequest request, int customerId); // Phương thức mới

        /// <summary>
        /// Xử lý callback (IPN) từ cổng thanh toán.
        /// </summary>
        /// <param name="callbackData">Dữ liệu nhận được từ cổng thanh toán (cần được xử lý tùy theo cổng).</param>
        /// <returns>Kết quả xử lý callback.</returns>
        Task<bool> HandlePaymentCallbackAsync(object callbackData); // object vì dữ liệu callback khác nhau tùy cổng

        /// <summary>
        /// Lấy trạng thái thanh toán của một giao dịch.
        /// </summary>
        /// <param name="transactionId">ID giao dịch nội bộ.</param>
        /// <returns>Trạng thái thanh toán.</returns>
        Task<Paymentstatus> GetTransactionStatusAsync(int transactionId);
        Task<int> GetCustomerIdByUserId(int userId);
        // Có thể thêm các hàm khác như Refund, GetPaymentDetails, v.v.
    }
}
