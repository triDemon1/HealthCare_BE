using HaNoiTravel.DTOS;

namespace HaNoiTravel.Interfaces
{
    public interface ILookupDataService
    {
        // Lookup Data (for dropdowns in admin forms)
        Task<IEnumerable<RoleDto>> GetAllRolesAsync();
        Task<IEnumerable<BookingStatusDto>> GetAllBookingStatusesAsync();
        Task<IEnumerable<OrderStatusDto>> GetAllOrderStatusAsync();
    }
}
