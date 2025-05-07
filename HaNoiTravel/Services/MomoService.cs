using HaNoiTravel.Data;
using HaNoiTravel.DTOS;
using HaNoiTravel.Interfaces;
using HaNoiTravel.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;

namespace HaNoiTravel.Services
{
    public class MomoService : IMomoService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<MomoService> _logger;
        private readonly string _momoApiUrl;
        private readonly string _partnerCode;
        private readonly string _accessKey; // Giữ lại AccessKey để dùng khi tạo request
        private readonly string _secretKey;
        private readonly string _returnUrl;
        private readonly string _ipnUrl;
        private readonly AppDbContext _context;

        public MomoService(HttpClient httpClient, IConfiguration configuration, ILogger<MomoService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;

            // Lấy thông tin cấu hình MoMo từ appsettings.json
            _momoApiUrl = _configuration["MomoSettings:ApiUrl"];
            _partnerCode = _configuration["MomoSettings:PartnerCode"];
            _accessKey = _configuration["MomoSettings:AccessKey"]; // Lấy AccessKey từ config
            _secretKey = _configuration["MomoSettings:SecretKey"];
            _returnUrl = _configuration["Payment:ReturnUrl"]; // Lấy từ cấu hình chung
            _ipnUrl = _configuration["MomoSettings:IpnUrl"]; // URL callback/IPN riêng cho MoMo

            // Cấu hình HttpClient nếu cần (BaseAddress, DefaultRequestHeaders...)
        }

        public async Task<MomoPaymentResponse> CreateWebPaymentAsync(string orderId, long amount, string orderInfo, string extraData)
        {
            if (string.IsNullOrEmpty(_momoApiUrl) || string.IsNullOrEmpty(_partnerCode) || string.IsNullOrEmpty(_accessKey) || string.IsNullOrEmpty(_secretKey) || string.IsNullOrEmpty(_returnUrl) || string.IsNullOrEmpty(_ipnUrl))
            {
                _logger.LogError("Momo configuration is missing.");
                return new MomoPaymentResponse { ResultCode = -1, Message = "Lỗi cấu hình MoMo." };
            }

            var requestId = Guid.NewGuid().ToString(); // ID duy nhất cho request
            var requestType = "captureWallet"; // Loại request Web Payment

            // 1. Tạo Raw Data để ký - Sử dụng các trường cần thiết cho Create Payment request
            // partnerCode={...}&accessKey={...}&requestId={...}&amount={...}&orderId={...}&orderInfo={...}&returnUrl={...}&ipnUrl={...}&extraData={...}&requestType={...}
            // THỨ TỰ CÁC TRƯỜNG TRONG CHUỖI KÝ RẤT QUAN TRỌNG - THAM KHẢO TÀI LIỆU MOМО API MỚI NHẤT
            var rawData = $"accessKey={_accessKey}&amount={amount}&extraData={extraData}&ipnUrl={_ipnUrl}&orderId={orderId}&orderInfo={orderInfo}&partnerCode={_partnerCode}&redirectUrl={_returnUrl}&requestId={requestId}&requestType={requestType}";

            // 2. Ký bằng Secret Key
            var signature = CalculateHmacSha256(rawData, _secretKey); // Hoặc MD5 tùy MoMo API version

            // 3. Tạo Request Body
            var requestBody = new MomoPaymentRequest
            {
                PartnerCode = _partnerCode,
                AccessKey = _accessKey, // Thêm AccessKey vào body request
                RequestId = requestId,
                Amount = amount,
                OrderId = orderId,
                OrderInfo = orderInfo,
                RedirectUrl = _returnUrl,
                IpnUrl = _ipnUrl,
                ExtraData = extraData,
                RequestType = requestType,
                Signature = signature
            };

            // 4. Gửi yêu cầu HTTP POST tới MoMo API
            try
            {
                var jsonBody = JsonConvert.SerializeObject(requestBody); // Hoặc System.Text.Json
                var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

                _logger.LogInformation($"Sending MoMo payment request to URL: {_momoApiUrl}"); // <-- LOG URL ĐÍCH
                _logger.LogInformation($"MoMo payment request payload: {jsonBody}"); // <-- LOG TOÀN BỘ PAYLOAD BAO GỒM orderId và signature
                var response = await _httpClient.PostAsync(_momoApiUrl, content);
                var responseString = await response.Content.ReadAsStringAsync();
                _logger.LogInformation($"Received MoMo payment response: {responseString}");


                if (response.IsSuccessStatusCode)
                {
                    var momoResponse = JsonConvert.DeserializeObject<MomoPaymentResponse>(responseString);
                    return momoResponse;
                }
                else
                {
                    _logger.LogError($"MoMo API returned error status code: {response.StatusCode}");
                    return new MomoPaymentResponse { ResultCode = -1, Message = $"MoMo API error: {response.StatusCode}" };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while calling MoMo API.");
                return new MomoPaymentResponse { ResultCode = -2, Message = $"Lỗi hệ thống: {ex.Message}" };
            }
        }

        // Hàm xử lý và xác thực chữ ký IPN MoMo
        public bool VerifyIpnSignature(MomoIpnRequest ipnData)
        {
            if (string.IsNullOrEmpty(_secretKey))
            {
                _logger.LogError("MoMo SecretKey is not configured for IPN verification.");
                return false;
            }
            if (ipnData == null)
            {
                _logger.LogError("Received null MomoIpnRequest for verification.");
                return false;
            }

            // 1. Tạo Raw Data từ dữ liệu IPN nhận được
            // THỨ TỰ CÁC TRƯỜNG TRONG CHUỖI KÝ RẤT QUAN TRỌNG - PHẢI KHỚP CHÍNH XÁC VỚI TÀI LIỆU MOМО API IPN MỚI NHẤT
            // Ví dụ phổ biến cho IPN thanh toán:
            // partnerCode={...}&orderId={...}&requestId={...}&amount={...}&message={...}&resultCode={...}&transId={...}&orderInfo={...}&extraData={...}
            // Cần kiểm tra lại tài liệu MoMo để đảm bảo thứ tự và tên trường
            try
            {
                var rawData = $"accessKey={_accessKey}" +
                                $"&amount={ipnData.Amount}" +
                                $"&extraData={ipnData.ExtraData}" +


                                $"&message={ipnData.Message}" +

                                $"&orderId={ipnData.OrderId}" +
                                $"&orderInfo={ipnData.OrderInfo}" +
                                $"&orderType={ipnData.OrderType}" +
                                $"&partnerCode={ipnData.PartnerCode}" +
                                $"&payType={ipnData.PayType}" +

                                $"&requestId={ipnData.RequestId}" +

                                $"&responseTime={ipnData.ResponseTime}" +

                                $"&resultCode={ipnData.ResultCode}" +
                                $"&transId={ipnData.TransId}";

                _logger.LogInformation($"MoMo IPN Raw Data for Signature Verification: {rawData}");


                // 2. Tính toán chữ ký của bạn từ Raw Data và Secret Key
                var calculatedSignature = CalculateHmacSha256(rawData, _secretKey); // Hoặc MD5 tùy MoMo API version

                // 3. So sánh chữ ký tính toán với chữ ký MoMo gửi về
                _logger.LogInformation($"Calculated Signature: {calculatedSignature}");
                _logger.LogInformation($"Received Signature: {ipnData.Signature}");

                return calculatedSignature.Equals(ipnData.Signature, StringComparison.OrdinalIgnoreCase);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error constructing raw data or calculating signature for MoMo IPN verification.");
                return false;
            }
        }

        // Hàm tính toán chữ ký HMACSHA256 (Giữ nguyên)
        private string CalculateHmacSha256(string data, string key)
        {
            var keyBytes = Encoding.UTF8.GetBytes(key);
            var dataBytes = Encoding.UTF8.GetBytes(data);

            using (var hmac = new HMACSHA256(keyBytes))
            {
                var hashBytes = hmac.ComputeHash(dataBytes);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower(); // Trả về hex string, lowercase
            }
        }
        // Cần implement CalculateMd5 nếu MoMo yêu cầu MD5
        // private string CalculateMd5(string data) { ... }


        public async Task<Paymentstatus> GetTransactionStatusAsync(int transactionId)
        {
            var transaction = await _context.Transactions
               .Include(t => t.PaymentStatus)
               .FirstOrDefaultAsync(t => t.TransactionId == transactionId);

            return transaction?.PaymentStatus;
        }
    }
}
