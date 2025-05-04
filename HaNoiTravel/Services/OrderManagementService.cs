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
        public async Task<IEnumerable<OrderAdminDto>> GetAllOrdersAsync()
        {
            var orders = await _context.Orders
               .Include(o => o.Customer)
               .Include(o => o.Orderstatus)
               .Include(o => o.Address)
               // .Include(o => o.OrderDetails).ThenInclude(od => od.Product) // Include OrderDetails and Product if needed
               .OrderByDescending(o => o.Orderdate)
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
                   // Map OrderDetails if included
                   // OrderDetails = o.OrderDetails.Select(od => new OrderDetailDto { ... }).ToList()
               })
               .ToListAsync();

            return orders;
        }

        public async Task<OrderAdminDto?> GetOrderByIdAsync(int orderId)
        {
            var order = await _context.Orders
               .Include(o => o.Customer)
               .Include(o => o.Orderstatus)
               .Include(o => o.Address)
               // .Include(o => o.OrderDetails).ThenInclude(od => od.Product) // Include OrderDetails and Product if needed
               .Where(o => o.Orderid == orderId)
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
                    // Map OrderDetails if included
                    // OrderDetails = o.OrderDetails.Select(od => new OrderDetailDto { ... }).ToList()
                })
               .FirstOrDefaultAsync();

            return order;
        }
        public async Task<OrderAdminDto?> UpdateOrderStatusAsync(int orderId, StatusUpdateDto statusDto)
        {
            var order = await _context.Orders
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

            order.Orderstatusid = statusDto.StatusId;
            order.Updatedat = DateTime.Now; // Assuming you have an UpdatedAt field

            await _context.SaveChangesAsync();

            // Fetch and return the updated order as DTO
            return await GetOrderByIdAsync(orderId); // Reuse GetOrderByIdAsync to get the DTO with relations
        }
    }
}
