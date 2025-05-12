using HaNoiTravel.Data;
using HaNoiTravel.DTOS;
using HaNoiTravel.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HaNoiTravel.Services
{
    public class OrderManagementService : IOrderManagementService
    {
        private readonly AppDbContext _context;
        public OrderManagementService(AppDbContext context)
        {
            _context = context;
        }
        public async Task<Pagination<OrderAdminDto>> GetAllOrdersAsync(int pageIndex, int pageSize)
        {
            var query = _context.Orders
               .Include(o => o.Customer)
               .Include(o => o.Orderstatus)
               .Include(o => o.Address)
               .OrderByDescending(o => o.Orderdate)
               .AsQueryable(); // Make it queryable for pagination

            var totalCount = await query.CountAsync();
            var items = await query
               .Skip(pageIndex * pageSize)
               .Take(pageSize)
               .Select(o => new OrderAdminDto
               {
                   OrderId = o.Orderid,
                   CustomerId = o.Customerid,
                   CustomerName = $"{o.Customer.Firstname} {o.Customer.Lastname}".Trim(),
                   OrderStatusId = o.Orderstatusid,
                   OrderStatusName = o.Orderstatus.Statusname,
                   AddressId = o.Addressid,
                   AddressStreet = o.Address.Street,
                   AddressWard = o.Address.Ward,
                   AddressDistrict = o.Address.District,
                   AddressCity = o.Address.City,
                   AddressCountry = o.Address.Country,
                   OrderDate = o.Orderdate,
                   TotalAmount = o.Totalamount,
                   CreatedAt = o.Createdat,
                   UpdatedAt = o.Updatedat
               })
               .ToListAsync();

            return new Pagination<OrderAdminDto>
            {
                Items = items,
                TotalCount = totalCount,
                PageIndex = pageIndex,
                PageSize = pageSize
            };
        }

        public async Task<OrderAdminDto?> GetOrderByIdAsync(int orderId)
        {
            var order = await _context.Orders
               .Include(o => o.Customer)
               .ThenInclude( c => c.User)
               .Include(o => o.Orderstatus)
               .Include(o => o.Address)
               // .Include(o => o.OrderDetails).ThenInclude(od => od.Product) // Include OrderDetails and Product if needed
               .Where(o => o.Orderid == orderId)
                .Select(o => new OrderAdminDto
                {
                    OrderId = o.Orderid,
                    CustomerId = o.Customerid,
                    CustomerName = $"{o.Customer.Firstname} {o.Customer.Lastname}".Trim(),
                    phoneNumber = o.Customer.User != null ? o.Customer.User.Phonenumber : null,
                    OrderStatusId = o.Orderstatusid,
                    OrderStatusName = o.Orderstatus.Statusname,
                    AddressId = o.Addressid,
                    AddressStreet = o.Address.Street,
                    AddressWard = o.Address.Ward,
                    AddressDistrict = o.Address.District,
                    AddressCity = o.Address.City,
                    AddressCountry = o.Address.Country,
                    OrderDate = o.Orderdate,
                    TotalAmount = o.Totalamount,
                    CreatedAt = o.Createdat,
                    UpdatedAt = o.Updatedat,
                    // Map OrderDetails if included
                    OrderDetails = o.Orderdetails.Select(od => new OrderDetailDto {
                        OrderDetailId = od.Orderdetailid,
                        ProductId = od.Productid,
                        ProductName = od.Product.Name, // Assuming Product has a Name property
                        Quantity = od.Quantity,
                        PriceAtPurchase = od.Priceatpurchase

                    }).ToList()
                })
               .FirstOrDefaultAsync();

            return order;
        }
        public async Task<OrderAdminDto?> UpdateOrderStatusAsync(int orderId, StatusUpdateDto statusDto)
        {
            var order = await _context.Orders
                                    .Include(o => o.Orderdetails) // Include order details to update stock
                                    .FirstOrDefaultAsync(o => o.Orderid == orderId);

            if (order == null)
            {
                return null; // Order not found
            }

            // Optional: Validate if the new StatusId exists
            var statusExists = await _context.Orderstatuses.AnyAsync(os => os.Orderstatusid == statusDto.StatusId);
            if (!statusExists)
            {
                throw new ArgumentException($"Order status with ID {statusDto.StatusId} not found.");
            }

            // Get the OrderStatus ID for 'Cancelled'
            var cancelledStatus = await _context.Orderstatuses
                                                .FirstOrDefaultAsync(os => os.Statusname == "Cancelled");

            // Get the current status before updating
            var currentStatusId = order.Orderstatusid;

            // Only update stock if the order was NOT already cancelled and IS being set to cancelled
            if (cancelledStatus != null && statusDto.StatusId == cancelledStatus.Orderstatusid && currentStatusId != cancelledStatus.Orderstatusid)
            {
                // If the order is being cancelled, return products to stock
                if (order.Orderdetails != null)
                {
                    foreach (var detail in order.Orderdetails)
                    {
                        var product = await _context.Products.FindAsync(detail.Productid);
                        if (product != null)
                        {
                            product.Stockquantity += detail.Quantity;
                        }
                    }
                    // Update all related products in a single call
                    _context.Products.UpdateRange(order.Orderdetails.Where(d => d.Product != null).Select(d => d.Product));
                }
            }

            order.Orderstatusid = statusDto.StatusId;
            order.Updatedat = DateTime.Now; // Assuming you have an UpdatedAt field

            await _context.SaveChangesAsync();

            // Fetch and return the updated order as DTO
            return await GetOrderByIdAsync(orderId); // Reuse GetOrderByIdAsync to get the DTO with relations
        }
    }
}
