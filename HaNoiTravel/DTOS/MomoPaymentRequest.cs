using Newtonsoft.Json;

namespace HaNoiTravel.DTOS
{
    public class MomoPaymentRequest
    {
        [JsonProperty("partnerCode")]
        public string PartnerCode { get; set; }

        [JsonProperty("accessKey")]
        public string AccessKey { get; set; }

        [JsonProperty("requestId")]
        public string RequestId { get; set; } // Unique ID cho mỗi request (có thể là TransactionID)

        [JsonProperty("amount")]
        public long Amount { get; set; } // MoMo thường dùng long cho số tiền (VNĐ)

        [JsonProperty("orderId")]
        public string OrderId { get; set; } // Unique ID cho mỗi đơn hàng/giao dịch (có thể là TransactionID)

        [JsonProperty("orderInfo")]
        public string OrderInfo { get; set; } // Thông tin mô tả đơn hàng

        [JsonProperty("redirectUrl")] // Sửa tên JsonProperty
        public string RedirectUrl { get; set; } // Sửa tên thuộc tính

        [JsonProperty("ipnUrl")]
        public string IpnUrl { get; set; } // URL nhận thông báo kết quả giao dịch từ MoMo

        [JsonProperty("extraData")]
        public string ExtraData { get; set; } // Dữ liệu thêm, thường là JSON string (vd: bookingId)

        [JsonProperty("requestType")]
        public string RequestType { get; set; } // Loại request, ví dụ: "captureWallet"

        [JsonProperty("signature")]
        public string Signature { get; set; } // Chữ ký an toàn
        // Các trường khác tùy cấu hình MoMo (vd: lang, autoCapture...)
    }
}
