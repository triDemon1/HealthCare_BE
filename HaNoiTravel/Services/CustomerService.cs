using HaNoiTravel.Data;
using HaNoiTravel.DTOS;
using HaNoiTravel.Models;
using HaNoiTravel.Interfaces;
using Microsoft.EntityFrameworkCore;
namespace HaNoiTravel.Services
{
    public class CustomerService: ICustomerService
    {
        private readonly AppDbContext _context;

        public CustomerService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<int?> GetCustomerIdByUserIdAsync(int userId)
        {
            var customer = await _context.Customers
                                        .AsNoTracking()
                                        .FirstOrDefaultAsync(c => c.Userid == userId);
            return customer?.Customerid; // Trả về CustomerId hoặc null
        }

        public async Task<bool> ValidateAddressOwnershipAsync(int addressId, int customerId)
        {
            return await _context.Addresses
                                 .AnyAsync(a => a.Addressid == addressId && a.Customerid == customerId);
        }
    }
}
