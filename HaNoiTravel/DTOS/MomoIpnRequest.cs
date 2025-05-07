using Newtonsoft.Json;

namespace HaNoiTravel.DTOS
{
    public class MomoIpnRequest
    {
        [JsonProperty("partnerCode")]
        public string PartnerCode { get; set; }

        [JsonProperty("orderId")]
        public string OrderId { get; set; }

        [JsonProperty("requestId")]
        public string RequestId { get; set; }

        [JsonProperty("amount")]
        public long Amount { get; set; }

        [JsonProperty("orderInfo")]
        public string OrderInfo { get; set; }

        [JsonProperty("orderType")]
        public string OrderType { get; set; } // Loại đơn hàng

        [JsonProperty("transId")]
        public long TransId { get; set; } // Mã giao dịch MoMo

        [JsonProperty("resultCode")]
        public int ResultCode { get; set; } // Mã kết quả giao dịch (0: thành công)

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("payType")]
        public string PayType { get; set; } // Phương thức thanh toán

        [JsonProperty("responseTime")]
        public long ResponseTime { get; set; }

        [JsonProperty("extraData")]
        public string ExtraData { get; set; } // Dữ liệu thêm ban đầu gửi đi

        [JsonProperty("signature")]
        public string Signature { get; set; } // Chữ ký để xác minh tính toàn vẹn
    }
}
