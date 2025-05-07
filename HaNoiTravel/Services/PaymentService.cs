using HaNoiTravel.Data;
using HaNoiTravel.DTOS;
using HaNoiTravel.Interfaces;
using HaNoiTravel.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace HaNoiTravel.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly AppDbContext _context;
        private readonly IMomoService _momoService; // Thay IPaymentGatewayService bằng IMomoService
        private readonly ILogger<PaymentService> _logger;

        public PaymentService(AppDbContext context, IMomoService momoService, ILogger<PaymentService> logger)
        {
            _context = context;
            _momoService = momoService;
            _logger = logger;
        }

        // Phương thức hiện tại cho Booking (có thể giữ nguyên hoặc refactor sau)
        public async Task<CreatePaymentResponse> CreatePaymentForBookingAsync(CreatePaymentRequest request, int customerId)
        {
            var booking = await _context.Bookings
                .Include(b => b.Service) // Cần Service để lấy tên cho orderInfo
                .FirstOrDefaultAsync(b => b.Bookingid == request.BookingId && b.Customerid == customerId);

            if (booking == null)
            {
                return new CreatePaymentResponse { Success = false, Message = "Booking không tồn tại hoặc không thuộc về khách hàng này.", ErrorCode = "BOOKING_NOT_FOUND" };
            }

            // Kiểm tra trạng thái booking (đã hủy, đã thanh toán)
            var cancelledBookingStatus = await _context.Bookingstatuses.FirstOrDefaultAsync(s => s.Statusname == "Cancelled");
            var paidPaymentStatus = await _context.Paymentstatuses.FirstOrDefaultAsync(s => s.Statusname == "Completed");

            if (booking.Statusid == cancelledBookingStatus?.Statusid)
            {
                return new CreatePaymentResponse { Success = false, Message = "Booking đã bị hủy.", ErrorCode = "BOOKING_CANCELLED" };
            }
            if (booking.PaymentStatusId == paidPaymentStatus?.Paymentstatusid)
            {
                return new CreatePaymentResponse { Success = false, Message = "Booking này đã được thanh toán.", ErrorCode = "BOOKING_ALREADY_PAID" };
            }

            // Thông tin cụ thể cho booking
            string itemType = "Booking";
            int itemId = booking.Bookingid;
            decimal amount = booking.Priceatbooking;
            string orderInfo = $"Thanh toan lich dat #{booking.Bookingid} - {booking.Service?.Name}";
            object extraDataObject = new { bookingId = booking.Bookingid };

            // Gọi hàm xử lý thanh toán chung
            return await ProcessPaymentCreationAsync(customerId, itemType, itemId, amount, orderInfo, extraDataObject);
        }

        // Phương thức MỚI cho Order
        public async Task<CreatePaymentResponse> CreatePaymentForOrderAsync(CreatePaymentForOrderRequest request, int customerId)
        {
            var order = await _context.Orders
                // .Include(o => o.Orderdetails) // Có thể include nếu cần thông tin chi tiết đơn hàng cho orderInfo
                .FirstOrDefaultAsync(o => o.Orderid == request.OrderId && o.Customerid == customerId);

            if (order == null)
            {
                return new CreatePaymentResponse { Success = false, Message = "Đơn hàng không tồn tại hoặc không thuộc về khách hàng này.", ErrorCode = "ORDER_NOT_FOUND" };
            }

            // Kiểm tra trạng thái đơn hàng (ví dụ: đã hủy)
            // Bảng Orders không có PaymentStatusId, chúng ta sẽ kiểm tra qua TransactionItems sau
            var cancelledOrderStatus = await _context.Orderstatuses.FirstOrDefaultAsync(s => s.Statusname == "Cancelled");
            if (order.Orderstatusid == cancelledOrderStatus?.Orderstatusid)
            {
                return new CreatePaymentResponse { Success = false, Message = "Đơn hàng đã bị hủy.", ErrorCode = "ORDER_CANCELLED" };
            }

            // Thông tin cụ thể cho order
            string itemType = "Order";
            int itemId = order.Orderid;
            decimal amount = order.Totalamount; // Lấy tổng tiền từ Order
            string orderInfo = $"Thanh toan cho don hang #{order.Orderid}"; // Thông tin đơn hàng cho MoMo
            object extraDataObject = new { orderId = order.Orderid }; // Dữ liệu gửi kèm cho MoMo (nếu cần)

            // Gọi hàm xử lý thanh toán chung
            return await ProcessPaymentCreationAsync(customerId, itemType, itemId, amount, orderInfo, extraDataObject);
        }

        // Phương thức chung để xử lý tạo thanh toán (được gọi bởi cả hai phương thức trên)
        private async Task<CreatePaymentResponse> ProcessPaymentCreationAsync(
            int customerId, string itemType, int itemId, decimal amount,
            string orderInfoForMomo, object extraDataForMomo)
        {
            var momoPaymentMethod = await _context.Paymentmethods.FirstOrDefaultAsync(pm => pm.Methodname == "MoMo"); // Hoặc "E-wallet" tùy theo DB
            if (momoPaymentMethod == null)
            {
                _logger.LogError($"PaymentMethod 'MoMo' not found in database.");
                return new CreatePaymentResponse { Success = false, Message = "Lỗi cấu hình phương thức thanh toán.", ErrorCode = "PAYMENT_METHOD_NOT_FOUND" };
            }

            var pendingPaymentStatus = await _context.Paymentstatuses.FirstOrDefaultAsync(s => s.Statusname == "Pending");
            if (pendingPaymentStatus == null)
            {
                _logger.LogError("PaymentStatus 'Pending' not found in database.");
                return new CreatePaymentResponse { Success = false, Message = "Lỗi cấu hình hệ thống thanh toán.", ErrorCode = "PAYMENT_STATUS_PENDING_NOT_FOUND" };
            }

            // Kiểm tra xem có giao dịch nào đang chờ xử lý hoặc đã hoàn thành cho item này không
            IQueryable<TransactionItem> query = _context.TransactionItems
                                                    .Include(ti => ti.Transaction)
                                                    .ThenInclude(t => t.PaymentStatus);
            if (itemType == "Booking")
            {
                query = query.Where(ti => ti.BookingId == itemId);
            }
            else if (itemType == "Order")
            {
                query = query.Where(ti => ti.OrderId == itemId);
            }

            var existingTransactionItems = await query.OrderByDescending(ti => ti.Transaction.Createdat).ToListAsync();
            bool isRetryAttempt = false;

            if (existingTransactionItems.Any())
            {
                var lastTransactionStatusName = existingTransactionItems.First().Transaction.PaymentStatus?.Statusname;
                if (lastTransactionStatusName == "Pending")
                {
                    return new CreatePaymentResponse { Success = true, Message = $"{itemType} này đang có giao dịch chờ thanh toán. Vui lòng kiểm tra hoặc thử lại sau.", ErrorCode = $"{itemType.ToUpper()}_HAS_PENDING_PAYMENT" };
                }
                if (lastTransactionStatusName == "Completed")
                {
                    return new CreatePaymentResponse { Success = false, Message = $"{itemType} này đã được thanh toán.", ErrorCode = $"{itemType.ToUpper()}_ALREADY_PAID" };
                }
                if (lastTransactionStatusName == "Failed" || lastTransactionStatusName == "Cancelled")
                {
                    isRetryAttempt = true;
                    _logger.LogInformation($"Previous transaction for {itemType} {itemId} was {lastTransactionStatusName}. Allowing new payment attempt.");
                }
                else if (!string.IsNullOrEmpty(lastTransactionStatusName)) // Các trạng thái khác không mong muốn
                {
                    _logger.LogWarning($"Previous transaction for {itemType} {itemId} has status {lastTransactionStatusName}. Not allowing new payment attempt.");
                    return new CreatePaymentResponse { Success = false, Message = $"Không thể tạo thanh toán mới cho {itemType} này do trạng thái giao dịch trước đó là {lastTransactionStatusName}.", ErrorCode = $"{itemType.ToUpper()}_INVALID_PREVIOUS_PAYMENT_STATUS" };
                }
            }

            // 2. Tạo bản ghi Transaction
            var transaction = new Transaction
            {
                CustomerId = customerId,
                TransactionDate = DateTime.Now,
                TotalAmount = amount,
                PaymentMethodId = momoPaymentMethod.Paymentmethodid,
                PaymentStatusId = pendingPaymentStatus.Paymentstatusid,
                Createdat = DateTime.Now
            };
            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync(); // Lưu để có TransactionID
            _logger.LogInformation($"--- NEW TRANSACTION CREATED --- {itemType} ID: {itemId}, Generated TransactionID: {transaction.TransactionId}");

            // 3. Tạo bản ghi TransactionItem
            var transactionItem = new TransactionItem
            {
                TransactionId = transaction.TransactionId,
                ItemAmount = amount
            };

            if (itemType == "Booking")
            {
                transactionItem.BookingId = itemId;
                // Cập nhật PaymentStatusId cho Booking (nếu có)
                var bookingToUpdate = await _context.Bookings.FindAsync(itemId);
                if (bookingToUpdate != null)
                {
                    bookingToUpdate.PaymentStatusId = pendingPaymentStatus.Paymentstatusid;
                    bookingToUpdate.Updatedat = DateTime.Now;
                }
            }
            else if (itemType == "Order")
            {
                transactionItem.OrderId = itemId;
                // Bảng Orders không có PaymentStatusId, trạng thái thanh toán theo dõi qua Transaction.
                // Có thể cập nhật Order.Updatedat nếu muốn
                var orderToUpdate = await _context.Orders.FindAsync(itemId);
                if (orderToUpdate != null)
                {
                    orderToUpdate.Updatedat = DateTime.Now;
                }
            }
            _context.TransactionItems.Add(transactionItem);
            await _context.SaveChangesAsync();

            // 5. Gửi yêu cầu đến MoMo
            var baseOrderIdForMomo = transaction.TransactionId.ToString();
            string orderIdForMomo = isRetryAttempt
                ? $"{baseOrderIdForMomo}_{DateTimeOffset.Now.ToUnixTimeMilliseconds()}"
                : baseOrderIdForMomo;

            _logger.LogInformation($"Payment attempt. Sending to MoMo: {orderIdForMomo} (Base: {baseOrderIdForMomo})");

            var amountInMomoFormat = (long)amount; // MoMo yêu cầu kiểu long
            var extraDataJson = JsonConvert.SerializeObject(extraDataForMomo);
            byte[] jsonBytes = System.Text.Encoding.UTF8.GetBytes(extraDataJson);
            string base64ExtraData = System.Convert.ToBase64String(jsonBytes);

            try
            {
                var momoResponse = await _momoService.CreateWebPaymentAsync(
                     orderIdForMomo,
                     amountInMomoFormat,
                     orderInfoForMomo, // Đã chuẩn bị ở phương thức gọi
                     base64ExtraData
                 );

                if (momoResponse != null && momoResponse.ResultCode == 0)
                {
                    transaction.Updatedat = DateTime.UtcNow;
                    // Cập nhật lại Updatedat cho item nếu cần
                    await _context.SaveChangesAsync();
                    return new CreatePaymentResponse
                    {
                        Success = true,
                        PaymentUrl = momoResponse.PayUrl,
                        TransactionRef = baseOrderIdForMomo, // Trả về TransactionID gốc
                        Message = momoResponse.Message
                    };
                }
                else
                {
                    var failedPaymentStatus = await _context.Paymentstatuses.FirstOrDefaultAsync(s => s.Statusname == "Failed");
                    if (failedPaymentStatus != null)
                    {
                        transaction.PaymentStatusId = failedPaymentStatus.Paymentstatusid;
                        transaction.Updatedat = DateTime.UtcNow;
                        if (itemType == "Booking" && transactionItem.BookingId.HasValue)
                        {
                            var bookingToUpdate = await _context.Bookings.FindAsync(transactionItem.BookingId.Value);
                            if (bookingToUpdate != null) bookingToUpdate.PaymentStatusId = failedPaymentStatus.Paymentstatusid;
                        }
                        // Không cần cập nhật trực tiếp PaymentStatus cho Order
                        await _context.SaveChangesAsync();
                    }
                    _logger.LogError($"MoMo Create Payment Error for {itemType} {itemId}: ResultCode {momoResponse?.ResultCode} - {momoResponse?.Message}");
                    return new CreatePaymentResponse
                    {
                        Success = false,
                        Message = momoResponse?.Message ?? "Có lỗi xảy ra khi kết nối với MoMo.",
                        ErrorCode = momoResponse?.ResultCode.ToString() ?? "MOMO_CREATE_ERROR"
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception during MoMo payment creation for {itemType} {itemId}");
                var failedPaymentStatus = await _context.Paymentstatuses.FirstOrDefaultAsync(s => s.Statusname == "Failed");
                if (failedPaymentStatus != null)
                {
                    transaction.PaymentStatusId = failedPaymentStatus.Paymentstatusid;
                    transaction.Updatedat = DateTime.UtcNow;
                    if (itemType == "Booking" && transactionItem.BookingId.HasValue)
                    {
                        var bookingToUpdate = await _context.Bookings.FindAsync(transactionItem.BookingId.Value);
                        if (bookingToUpdate != null) bookingToUpdate.PaymentStatusId = failedPaymentStatus.Paymentstatusid;
                    }
                    await _context.SaveChangesAsync();
                }
                return new CreatePaymentResponse { Success = false, Message = "Lỗi hệ thống khi xử lý thanh toán.", ErrorCode = "SYSTEM_ERROR" };
            }
        }


        // Phương thức HandlePaymentCallbackAsync (XỬ LÝ IPN)
        public async Task<bool> HandlePaymentCallbackAsync(object callbackData)
        {
            _logger.LogInformation("Received MoMo IPN callback.");
            MomoIpnRequest ipnData;
            try
            {
                ipnData = JsonConvert.DeserializeObject<MomoIpnRequest>(callbackData.ToString());
                _logger.LogInformation($"Parsed Momo IPN Data: {JsonConvert.SerializeObject(ipnData)}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to parse MoMo IPN data.");
                return false;
            }

            if (ipnData == null)
            {
                _logger.LogError("Received null MomoIpnRequest.");
                return false;
            }

            if (!_momoService.VerifyIpnSignature(ipnData))
            {
                _logger.LogWarning($"MoMo IPN signature verification failed for OrderId: {ipnData.OrderId}. Received Signature: {ipnData.Signature}");
                return false;
            }
            _logger.LogInformation($"MoMo IPN signature verified successfully for OrderId: {ipnData.OrderId}");

            string momoOrderId = ipnData.OrderId;
            string[] parts = momoOrderId.Split('_');
            int transactionId; // Đây là TransactionID nội bộ của bạn

            if (!(parts.Length > 0 && int.TryParse(parts[0], out transactionId)))
            {
                _logger.LogError($"Could not parse valid TransactionId from MoMo OrderId: {momoOrderId}");
                return false;
            }
            _logger.LogInformation($"Parsed TransactionID from MoMo OrderId: {transactionId}");

            var transaction = await _context.Transactions
                .Include(t => t.TransactionItems)
                    .ThenInclude(ti => ti.Booking) // Include Booking
                .Include(t => t.TransactionItems)
                    .ThenInclude(ti => ti.Order) // Include Order
                .Include(t => t.PaymentStatus)
                .FirstOrDefaultAsync(t => t.TransactionId == transactionId);

            if (transaction == null)
            {
                _logger.LogError($"Transaction with ID {transactionId} not found for MoMo IPN.");
                return false;
            }

            var currentPaymentStatusName = transaction.PaymentStatus?.Statusname;
            if (currentPaymentStatusName == "Completed" || currentPaymentStatusName == "Failed" || currentPaymentStatusName == "Refunded")
            {
                _logger.LogInformation($"MoMo IPN received for transaction {transactionId} which is already in status: {currentPaymentStatusName}. Ignoring.");
                return true; // Đã xử lý rồi, không cần làm gì thêm
            }

            int momoResultCode = ipnData.ResultCode;
            string statusNameBasedOnMomo;
            if (momoResultCode == 0) statusNameBasedOnMomo = "Completed";
            else if (momoResultCode == 1006) statusNameBasedOnMomo = "Cancelled"; // User hủy trên MoMo
            else statusNameBasedOnMomo = "Failed";

            var newPaymentStatus = await _context.Paymentstatuses.FirstOrDefaultAsync(s => s.Statusname == statusNameBasedOnMomo);
            if (newPaymentStatus == null)
            {
                _logger.LogError($"PaymentStatus for '{statusNameBasedOnMomo}' (MoMo ResultCode {momoResultCode}) not found in database.");
                return false;
            }

            transaction.PaymentStatusId = newPaymentStatus.Paymentstatusid;
            transaction.TransactionIdPaymentGateway = ipnData.TransId.ToString(); // Lưu transId của MoMo
            transaction.Updatedat = DateTime.UtcNow;
            _logger.LogInformation($"MoMo IPN: Transaction {transactionId} updated to status {newPaymentStatus.Statusname} (MoMo ResultCode: {momoResultCode}).");

            // Cập nhật trạng thái cho Booking hoặc Order liên quan
            if (transaction.TransactionItems != null)
            {
                foreach (var item in transaction.TransactionItems)
                {
                    if (item.BookingId.HasValue && item.Booking != null)
                    {
                        item.Booking.PaymentStatusId = newPaymentStatus.Paymentstatusid;
                        item.Booking.Updatedat = DateTime.UtcNow;
                        _logger.LogInformation($"Booking {item.Booking.Bookingid} PaymentStatus updated to {newPaymentStatus.Statusname}.");
                        // (Tùy chọn) Cập nhật BookingStatus nếu cần, ví dụ: nếu thanh toán thành công -> Confirmed
                        // if (newPaymentStatus.Statusname == "Completed") {
                        //     var confirmedBookingStatus = await _context.Bookingstatuses.FirstOrDefaultAsync(bs => bs.Statusname == "Confirmed");
                        //     if (confirmedBookingStatus != null) item.Booking.Statusid = confirmedBookingStatus.Statusid;
                        // }
                    }
                    else if (item.OrderId.HasValue && item.Order != null)
                    {
                        // Bảng Order không có PaymentStatusId. Trạng thái thanh toán theo dõi qua Transaction.
                        // Cập nhật OrderStatusId của Order dựa trên kết quả thanh toán.
                        item.Order.Updatedat = DateTime.UtcNow;
                        if (newPaymentStatus.Statusname == "Completed")
                        {
                            var processingOrderStatus = await _context.Orderstatuses.FirstOrDefaultAsync(os => os.Statusname == "Processing"); // Hoặc "Paid", "Confirmed"
                            if (processingOrderStatus != null)
                            {
                                item.Order.Orderstatusid = processingOrderStatus.Orderstatusid;
                                _logger.LogInformation($"Order {item.Order.Orderid} OrderStatus updated to '{processingOrderStatus.Statusname}'.");
                            }
                        }
                        else if (newPaymentStatus.Statusname == "Failed" || newPaymentStatus.Statusname == "Cancelled")
                        {
                            var pendingOrderStatus = await _context.Orderstatuses.FirstOrDefaultAsync(os => os.Statusname == "Pending"); // Hoặc một trạng thái "Payment Failed"
                            if (pendingOrderStatus != null)
                            {
                                // item.Order.Orderstatusid = pendingOrderStatus.Orderstatusid; // Giữ nguyên Pending hoặc chuyển sang trạng thái lỗi thanh toán
                                _logger.LogInformation($"Order {item.Order.Orderid} payment {newPaymentStatus.Statusname}. OrderStatus remains/updated accordingly.");
                            }
                            // TODO: Xem xét có cần rollback số lượng sản phẩm nếu thanh toán thất bại không (hiện tại đã trừ ở OrderService)
                        }
                    }
                }
            }
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<Paymentstatus> GetTransactionStatusAsync(int transactionId)
        {
            var transaction = await _context.Transactions
               .Include(t => t.PaymentStatus)
               .FirstOrDefaultAsync(t => t.TransactionId == transactionId);

            return transaction?.PaymentStatus;
        }

        public async Task<int> GetCustomerIdByUserId(int userId)
        {
            _logger.LogInformation($"Fetching CustomerId for UserId: {userId}");
            // Đây là phần bạn cần truy vấn DB để lấy CustomerId từ UserId
            // Ví dụ:
            var customer = _context.Customers.FirstOrDefault(c => c.Userid == userId);
            return customer?.Customerid ?? 0;
        }
    }
}
