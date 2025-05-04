using HaNoiTravel.Data;
using HaNoiTravel.DTOS;
using HaNoiTravel.Interfaces;
using HaNoiTravel.Models;
using Microsoft.EntityFrameworkCore; // Cần cho FirstOrDefaultAsync

namespace HaNoiTravel.Services
{
    public class LookupDataService : ILookupDataService
    {
        private readonly AppDbContext _context;
        public LookupDataService(AppDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<RoleDto>> GetAllRolesAsync()
        {
            return await _context.Roles
                .Select(r => new RoleDto { RoleId = r.Roleid, RoleName = r.Rolename })
                .ToListAsync();
        }

        public async Task<IEnumerable<BookingStatusDto>> GetAllBookingStatusesAsync()
        {
            return await _context.Bookingstatuses
               .Select(bs => new BookingStatusDto { StatusId = bs.Statusid, StatusName = bs.Statusname })
               .ToListAsync();
        }

        public async Task<IEnumerable<OrderStatusDto>> GetAllOrderStatusAsync()
        {
            return await _context.Orderstatuses
               .Select(os => new OrderStatusDto { OrderStatusId = os.Orderstatusid, StatusName = os.Statusname })
               .ToListAsync();
        }
    }
}
