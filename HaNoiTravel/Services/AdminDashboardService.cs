using HaNoiTravel.Data;
using HaNoiTravel.DTOS;
using HaNoiTravel.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HaNoiTravel.Services
{
    public class AdminDashboardService : IAdminDashboardService
    {
        private readonly AppDbContext _context;

        public AdminDashboardService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<DashboardStatsDto> GetDashboardStatsAsync()
        {
            var today = DateTime.Today;
            var yesterday = today.AddDays(-1);
            var startOfMonth = new DateTime(today.Year, today.Month, 1);
            var startOfLastMonth = startOfMonth.AddMonths(-1);
            var endOfLastMonth = startOfMonth.AddDays(-1);

            // Tổng số người dùng hiện tại và tháng trước
            var totalUsers = await _context.Users.CountAsync(u => u.Isactive);
            var lastMonthUsers = await _context.Users
                .Where(u => u.Createdat >= startOfLastMonth && u.Createdat <= endOfLastMonth)
                .CountAsync();

            // Lịch đặt hôm nay & hôm qua
            var todayBookings = await _context.Bookings
                .CountAsync(b => b.Createdat >= today && b.Createdat < today.AddDays(1));

            var yesterdayBookings = await _context.Bookings
                .CountAsync(b => b.Createdat >= yesterday && b.Createdat < today);

            // Doanh thu tháng này & tháng trước
            var monthlyRevenue = await _context.Transactions
                .Where(t => t.TransactionDate >= startOfMonth)
                .SumAsync(t => (decimal?)t.TotalAmount) ?? 0;

            var lastMonthRevenue = await _context.Transactions
                .Where(t => t.TransactionDate >= startOfLastMonth && t.TransactionDate <= endOfLastMonth)
                .SumAsync(t => (decimal?)t.TotalAmount) ?? 0;

            // Dịch vụ nổi bật tháng này
            var topService = await _context.Bookings
                            .Where(b => b.Scheduledstarttime >= startOfMonth && b.PaymentStatusId == 2)
                            .GroupBy(b => b.Serviceid)
                            .OrderByDescending(g => g.Count())
                            .Select(g => new
                            {
                                ServiceId = g.Key,
                                Count = g.Count()
                            })
                            .FirstOrDefaultAsync();

            string topServiceName = "Không có dữ liệu";
            int topServiceBookings = 0;

            if (topService != null)
            {
                var service = await _context.Services.FindAsync(topService.ServiceId);
                topServiceName = service?.Name ?? "Không rõ";
                topServiceBookings = topService.Count;
            }

            return new DashboardStatsDto
            {
                TotalUsers = totalUsers,
                LastMonthUsers = lastMonthUsers,
                TodayBookings = todayBookings,
                YesterdayBookings = yesterdayBookings,
                MonthlyRevenue = monthlyRevenue,
                LastMonthRevenue = lastMonthRevenue,
                TopServiceName = topServiceName,
                TopServiceBookings = topServiceBookings
            };
        }
        public async Task<List<BookingsBySubjectTypeDto>> GetBookingsBySubjectTypeAsync()
        {
            var startOfMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

            var result = await (from booking in _context.Bookings
                                join subject in _context.Subjects on booking.Subjectid equals subject.Subjectid
                                join type in _context.Subjecttypes on subject.Typeid equals type.Typeid
                                where booking.Scheduledstarttime >= startOfMonth
                                group booking by type.Subjectname into g
                                select new BookingsBySubjectTypeDto
                                {
                                    SubjectTypeName = g.Key,
                                    BookingCount = g.Count()
                                }).ToListAsync();

            return result;
        }
        public async Task<List<TopServiceDto>> GetTopBookedServicesAsync(int top = 5)
        {
            var startOfMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

            var result = await _context.Bookings
                .Where(b => b.Scheduledstarttime >= startOfMonth && b.PaymentStatusId == 2) // chỉ lấy đã thanh toán
                .GroupBy(b => b.Serviceid)
                .Select(g => new {
                    ServiceId = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .Take(top)
                .Join(_context.Services,
                      g => g.ServiceId,
                      s => s.Serviceid,
                      (g, s) => new TopServiceDto
                      {
                          Name = s.Name,
                          Count = g.Count
                      })
                .ToListAsync();

            return result;
        }

    }
}
