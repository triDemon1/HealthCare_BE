using HaNoiTravel.DTOS;

namespace HaNoiTravel.Interfaces
{
    public interface IAdminDashboardService
    {
        Task<DashboardStatsDto> GetDashboardStatsAsync();
        Task<List<BookingsBySubjectTypeDto>> GetBookingsBySubjectTypeAsync();
        Task<List<TopServiceDto>> GetTopBookedServicesAsync(int top = 5);

    }
}
