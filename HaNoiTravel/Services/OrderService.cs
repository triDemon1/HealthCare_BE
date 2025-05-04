using HaNoiTravel.Data;
using HaNoiTravel.DTOS;
using HaNoiTravel.Models;
using HaNoiTravel.Interfaces;
using System.Security.Claims;

namespace HaNoiTravel.Services
{
    public class OrderService: IOrderService
    {
        private readonly AppDbContext _context;
        private readonly ICustomerService _customerService; // Inject service khác nếu cần

        public OrderService(AppDbContext context, ICustomerService customerService)
        {
            _context = context;
            _customerService = customerService;
        }

        public async Task<(int? OrderId, string? ErrorMessage)> CreateOrderAsync(CreateOrderDto createOrderDto, ClaimsPrincipal user)
        {
            // 1. Lấy User ID từ ClaimsPrincipal
            var userIdClaim = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return (null, "User ID not found in token.");
            }

            // 2. Lấy Customer ID từ User ID (Sử dụng CustomerService)
            var customerId = await _customerService.GetCustomerIdByUserIdAsync(userId);
            if (!customerId.HasValue)
            {
                return (null, "Customer profile not found for the logged-in user.");
            }

            // 3. Kiểm tra AddressId (Sử dụng CustomerService)
            var isAddressValid = await _customerService.ValidateAddressOwnershipAsync(createOrderDto.AddressId, customerId.Value);
            if (!isAddressValid)
            {
                return (null, "Invalid or unauthorized address ID.");
            }

            if (createOrderDto.Items == null || !createOrderDto.Items.Any())
            {
                return (null, "Order must contain at least one item.");
            }

            // --- Bắt đầu Transaction ---
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                decimal totalAmount = 0;
                var orderDetails = new List<Orderdetail>();
                var productUpdates = new List<Product>(); // Lưu các sản phẩm cần cập nhật tồn kho

                // 4. Kiểm tra tồn kho và tính tổng tiền
                foreach (var itemDto in createOrderDto.Items)
                {
                    // Dùng FindAsync để tận dụng caching nếu có
                    var product = await _context.Products.FindAsync(itemDto.ProductId);
                    if (product == null || !product.Isactive)
                    {
                        await transaction.RollbackAsync();
                        return (null, $"Product with ID {itemDto.ProductId} not found or inactive.");
                    }
                    if (product.Stockquantity < itemDto.Quantity)
                    {
                        await transaction.RollbackAsync();
                        return (null, $"Insufficient stock for product '{product.Name}'. Available: {product.Stockquantity}, Requested: {itemDto.Quantity}.");
                    }

                    // Chuẩn bị cập nhật tồn kho
                    product.Stockquantity -= itemDto.Quantity;
                    productUpdates.Add(product); // Thêm vào danh sách để cập nhật sau

                    var priceAtPurchase = product.Price;
                    totalAmount += priceAtPurchase * itemDto.Quantity;

                    orderDetails.Add(new Orderdetail
                    {
                        Productid = itemDto.ProductId,
                        Quantity = itemDto.Quantity,
                        Priceatpurchase = priceAtPurchase
                    });
                }

                // 5. Tạo Order mới
                var newOrder = new Order
                {
                    Customerid = customerId.Value,
                    Addressid = createOrderDto.AddressId,
                    Orderstatusid = 1, // Pending
                    Orderdate = DateTime.UtcNow,
                    Totalamount = totalAmount,
                    Orderdetails = orderDetails
                };

                _context.Orders.Add(newOrder);
                _context.Products.UpdateRange(productUpdates); // Cập nhật tất cả sản phẩm cùng lúc

                await _context.SaveChangesAsync();

                // --- Commit Transaction ---
                await transaction.CommitAsync();

                return (newOrder.Orderid, null); // Trả về OrderId và không có lỗi
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                // Log lỗi (ex)
                Console.WriteLine($"Error creating order: {ex.Message}"); // Log đơn giản
                return (null, "An internal error occurred while processing the order.");
            }
        }
    }
}
