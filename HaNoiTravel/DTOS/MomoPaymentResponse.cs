using Newtonsoft.Json;

namespace HaNoiTravel.DTOS
{
    public class MomoPaymentResponse
    {
        [JsonProperty("partnerCode")]
        public string PartnerCode { get; set; }

        [JsonProperty("orderId")]
        public string OrderId { get; set; }

        [JsonProperty("requestId")]
        public string RequestId { get; set; }

        [JsonProperty("amount")]
        public long Amount { get; set; }

        [JsonProperty("responseTime")]
        public long ResponseTime { get; set; } // Timestamp

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("resultCode")]
        public int ResultCode { get; set; } // Mã kết quả từ MoMo (0: thành công)

        [JsonProperty("payUrl")]
        public string PayUrl { get; set; } // URL redirect người dùng đến trang thanh toán MoMo

        [JsonProperty("deeplink")]
        public string Deeplink { get; set; } // Deeplink cho ứng dụng MoMo
        // Các trường khác tùy MoMo
    }
}
