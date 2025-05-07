using HaNoiTravel.Data;
using HaNoiTravel.DTOS;
using HaNoiTravel.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Security.Claims;
using System.Text;

namespace HaNoiTravel.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<PaymentsController> _logger; // Inject Logger

        public PaymentsController(IPaymentService paymentService, IConfiguration configuration, ILogger<PaymentsController> logger, AppDbContext context)
        {
            _paymentService = paymentService;
            _configuration = configuration;
            _logger = logger;
            _context = context;
        }

        [HttpPost("create-for-booking")]
        [Authorize(Roles = "Customer")] // Chỉ Customer mới được tạo thanh toán
        public async Task<IActionResult> CreatePaymentForBooking([FromBody] CreatePaymentRequest request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized("Người dùng chưa đăng nhập hoặc không có thông tin User ID.");
            }

            int customerId = GetCustomerIdByUserId(userId);
            if (customerId <= 0)
            {
                return Unauthorized("Không tìm thấy thông tin khách hàng.");
            }

            // Lấy ReturnUrl và CancelUrl từ cấu hình
            request.ReturnUrl = _configuration["Payment:ReturnUrl"];
            string frontendCancelUrl = _configuration["Payment:FrontendCancelUrl"];

            if (string.IsNullOrEmpty(request.ReturnUrl) || string.IsNullOrEmpty(frontendCancelUrl))
            {
                _logger.LogError("Payment Return/Cancel URLs are not configured.");
                return StatusCode(500, new CreatePaymentResponse { Success = false, Message = "Lỗi cấu hình URL thanh toán.", ErrorCode = "CONFIG_ERROR" });
            }


            // PaymentMethodId cho MoMo sẽ được set trong PaymentService dựa trên tên 'MoMo' hoặc 'E-wallet'

            var response = await _paymentService.CreatePaymentForBookingAsync(request, customerId);

            if (response.Success)
            {
                return Ok(response); // Trả về URL thanh toán của MoMo
            }
            else
            {
                return BadRequest(response); // Trả về lỗi nếu khởi tạo MoMo thất bại
            }
        }
        // ENDPOINT MỚI CHO ORDER
        [HttpPost("create-for-order")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> CreatePaymentForOrder([FromBody] CreatePaymentForOrderRequest request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized("Người dùng chưa đăng nhập hoặc không có thông tin User ID.");
            }

            // int customerId = GetCustomerIdByUserId(userId); // Cách cũ
            int customerId = await _paymentService.GetCustomerIdByUserId(userId); // Gọi từ service
            if (customerId <= 0)
            {
                return Unauthorized("Không tìm thấy thông tin khách hàng.");
            }

            // Gán ReturnUrl từ config, tương tự như cho booking
            request.ReturnUrl = _configuration["Payment:ReturnUrl"];
            // request.CancelUrl = _configuration["Payment:FrontendCancelUrl"]; // Nếu DTO mới có

            var response = await _paymentService.CreatePaymentForOrderAsync(request, customerId);

            if (response.Success) return Ok(response);
            return BadRequest(response);
        }
        // Endpoint nhận IPN từ MoMo
        [HttpPost("momo-ipn")]
        [AllowAnonymous] // MoMo gọi đến endpoint này, không cần xác thực người dùng
        [ApiExplorerSettings(IgnoreApi = true)] // Ẩn khỏi Swagger
        public async Task<IActionResult> MomoIpn()
        {
            _logger.LogInformation("MoMo IPN received.");
            // Đọc dữ liệu IPN từ Request Body
            string requestBody;
            using (var reader = new StreamReader(Request.Body, Encoding.UTF8))
            {
                requestBody = await reader.ReadToEndAsync();
            }

            _logger.LogInformation($"MoMo IPN Body: {requestBody}");

            // Dữ liệu IPN của MoMo có thể là JSON hoặc form-urlencoded tùy phiên bản API
            // Cần parse requestBody sang object MomoIpnRequest
            object ipnData = null; // Cần xử lý requestBody để tạo object này
            try
            {
                // Ví dụ nếu MoMo gửi JSON:
                ipnData = JObject.Parse(requestBody); // Sử dụng JObject để dễ dàng parse, sau đó có thể map sang DTO
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to parse MoMo IPN request body.");
                return BadRequest("Invalid IPN data format."); // Trả về lỗi để MoMo gửi lại
            }


            // Gọi PaymentService để xử lý IPN
            // PaymentService sẽ xác thực chữ ký và cập nhật DB
            bool result = await _paymentService.HandlePaymentCallbackAsync(ipnData); // Truyền object đã parse

            if (result)
            {
                // Trả về response thành công theo yêu cầu của MoMo (thường là HTTP 200 OK)
                // MoMo có thể yêu cầu trả về chuỗi cụ thể, kiểm tra tài liệu
                _logger.LogInformation("MoMo IPN processed successfully.");
                return Ok("Received"); // Ví dụ MoMo yêu cầu trả về "Received"
            }
            else
            {
                // Trả về response lỗi nếu xử lý thất bại (ví dụ: chữ ký không hợp lệ)
                _logger.LogError("MoMo IPN processing failed.");
                return BadRequest("Failed"); // Ví dụ MoMo yêu cầu trả về "Failed"
            }
        }

        // Endpoint Return URL (người dùng được chuyển về sau khi thanh toán)
        [HttpGet("momo-return")]
        [AllowAnonymous]
        public async Task<IActionResult> MomoReturn(
            [FromQuery] string partnerCode, // Thêm các param bạn nhận được từ MoMo
            [FromQuery] string orderId,     // orderId có thể chứa hậu tố
            [FromQuery] string requestId,
            [FromQuery] long amount,
            [FromQuery] string orderInfo,
            [FromQuery] string orderType,
            [FromQuery] string transId,     // Mã giao dịch bên MoMo
            [FromQuery] int resultCode,     // Mã kết quả giao dịch từ MoMo
            [FromQuery] string message,     // Thông báo từ MoMo
           // [FromQuery] string payType,
            [FromQuery] long responseTime,
            [FromQuery] string extraData,   // Dữ liệu thêm (bao gồm bookingId)
            [FromQuery] string signature    // Chữ ký từ MoMo
            )
        {
            _logger.LogInformation($"MoMo Return URL hit for OrderId: {orderId}, ResultCode: {resultCode}");

            // --- PHÂN TÍCH ORDERID ĐỂ LẤY TRANSACTIONID GỐC ---
            // orderId có thể có dạng "TransactionID_Timestamp"
            string momoOrderId = orderId; // orderId nhận trực tiếp từ query string
            string[] parts = momoOrderId.Split('_');
            int transactionId;

            if (parts.Length > 0 && int.TryParse(parts[0], out transactionId))
            {
                // transactionId giờ là ID gốc (ví dụ: 28)
                _logger.LogInformation($"Parsed TransactionID from MoMo Return URL OrderId: {transactionId}");
            }
            else
            {
                // Trường hợp orderId không có hậu tố hoặc không phải dạng số_timestamp
                // Thử parse trực tiếp
                if (!int.TryParse(momoOrderId, out transactionId))
                {
                    _logger.LogError($"Could not parse valid TransactionId from MoMo Return URL OrderId: {momoOrderId}");
                    // Redirect về trang lỗi chung ở frontend nếu không parse được OrderId
                    // Sử dụng FrontendFailedUrl cho trường hợp này
                    return Redirect($"{_configuration["Payment:FrontendFailedUrl"]}?error=invalid_transaction_ref");
                }
                _logger.LogInformation($"Parsed TransactionID directly from MoMo Return URL OrderId: {transactionId}");
            }
            // --- Kết thúc phân tích ---


            // Lúc này transactionId đã chứa ID nội bộ (ví dụ: 28)
            // Tiếp tục logic lấy trạng thái giao dịch từ DB (đã được cập nhật bởi IPN)
            // Lưu ý: IPN có thể đến TRƯỚC hoặc SAU Return URL.
            // Endpoint return chỉ để hiển thị thông báo, không phải nơi cập nhật trạng thái chính.
            // Trạng thái chính đã được cập nhật bởi IPN trong HandlePaymentCallbackAsync.

            var transactionStatus = await _paymentService.GetTransactionStatusAsync(transactionId); // Lấy trạng thái sau khi IPN đã xử lý

            string frontendRedirectUrl;
            string frontendSuccessUrl = _configuration["Payment:FrontendSuccessUrl"];
            string frontendFailedUrl = _configuration["Payment:FrontendFailedUrl"];
            string frontendCancelUrl = _configuration["Payment:FrontendCancelUrl"]; // URL Frontend cho trường hợp hủy

            // Kiểm tra các URL Frontend có được cấu hình không
            if (string.IsNullOrEmpty(frontendSuccessUrl) || string.IsNullOrEmpty(frontendFailedUrl) || string.IsNullOrEmpty(frontendCancelUrl))
            {
                _logger.LogError("Frontend Return URLs (Success/Failed/Cancel) are not configured.");
                return StatusCode(500, "Lỗi cấu hình URL frontend trả về.");
            }


            if (transactionStatus != null)
            {
                // Dựa vào trạng thái trong DB (sau IPN)
                switch (transactionStatus.Statusname)
                {
                    case "Completed":
                        frontendRedirectUrl = $"{frontendSuccessUrl}?transactionId={transactionId}";
                        break;
                    case "Failed":
                        // Nếu trạng thái trong DB là Failed, redirect đến trang Failed
                        frontendRedirectUrl = $"{frontendFailedUrl}?transactionId={transactionId}&momoResultCode={resultCode}";
                        break;
                    case "Cancelled": // Giả định IPN set trạng thái Cancelled
                                      // Nếu trạng thái trong DB là Cancelled, redirect đến trang Cancelled
                        frontendRedirectUrl = $"{frontendCancelUrl}?transactionId={transactionId}&momoResultCode={resultCode}";
                        break;
                    case "Pending":
                        // Nếu trạng thái vẫn là Pending (có thể IPN chưa tới hoặc lỗi IPN)
                        // Dựa vào resultCode từ MoMo để phán đoán
                        if (resultCode == 0) // MoMo báo thành công (nhưng IPN chưa tới?)
                        {
                            // Redirect về trang success và frontend sẽ chờ hoặc kiểm tra lại trạng thái
                            frontendRedirectUrl = $"{frontendSuccessUrl}?transactionId={transactionId}&status=pending_ipn"; // Thêm param báo IPN Pending
                        }
                        else if (resultCode == 1006) // MoMo báo hủy bởi user
                        {
                            frontendRedirectUrl = $"{frontendCancelUrl}?transactionId={transactionId}&momoResultCode={resultCode}";
                        }
                        else // Các resultCode khác (thất bại)
                        {
                            frontendRedirectUrl = $"{frontendFailedUrl}?transactionId={transactionId}&momoResultCode={resultCode}";
                        }
                        _logger.LogWarning($"Transaction {transactionId} is still Pending in DB on Return URL hit (MoMo ResultCode: {resultCode}). Redirecting based on ResultCode.");
                        break;
                    default:
                        // Trạng thái không xác định, redirect đến trang lỗi
                        _logger.LogError($"Transaction {transactionId} is in unexpected status '{transactionStatus.Statusname}' on Return URL hit.");
                        frontendRedirectUrl = $"{frontendFailedUrl}?transactionId={transactionId}&error=unexpected_status&dbStatus={transactionStatus.Statusname}";
                        break;
                }
            }
            else
            {
                _logger.LogError($"Transaction with ID {transactionId} not found in DB on Return URL hit. Redirecting to failure page.");
                // Không tìm thấy giao dịch -> coi là lỗi
                frontendRedirectUrl = $"{frontendFailedUrl}?transactionId={transactionId}&error=transaction_not_found";
            }

            _logger.LogInformation($"MoMo Return: Redirecting user to frontend URL: {frontendRedirectUrl}");
            return Redirect(frontendRedirectUrl); // <-- Redirect về Frontend
        }

        private int GetCustomerIdByUserId(int userId)
        {
            _logger.LogInformation($"Fetching CustomerId for UserId: {userId}");
            // Đây là phần bạn cần truy vấn DB để lấy CustomerId từ UserId
            // Ví dụ:
            var customer = _context.Customers.FirstOrDefault(c => c.Userid == userId);
            return customer?.Customerid ?? 0;
        }
    }
}
