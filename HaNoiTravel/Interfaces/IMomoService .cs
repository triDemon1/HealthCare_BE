using HaNoiTravel.DTOS;

namespace HaNoiTravel.Interfaces
{
    public interface IMomoService
    {
        /// <summary>
        /// Tạo yêu cầu thanh toán Web Payment tới MoMo.
        /// </summary>
        /// <param name="orderId">ID đơn hàng nội bộ (TransactionID).</param>
        /// <param name="amount">Số tiền cần thanh toán.</param>
        /// <param name="orderInfo">Thông tin mô tả đơn hàng.</param>
        /// <param name="extraData">Dữ liệu thêm (vd: bookingId).</param>
        /// <returns>Response từ MoMo bao gồm PayUrl.</returns>
        Task<MomoPaymentResponse> CreateWebPaymentAsync(string orderId, long amount, string orderInfo, string extraData);

        /// <summary>
        /// Xử lý và xác thực dữ liệu IPN nhận từ MoMo.
        /// </summary>
        /// <param name="ipnData">Dữ liệu IPN từ MoMo.</param>
        /// <returns>True nếu xác thực và xử lý thành công.</returns>
        bool VerifyIpnSignature(MomoIpnRequest ipnData);
        // Có thể thêm hàm xử lý callback logic ở đây hoặc ở PaymentService
    }
}
