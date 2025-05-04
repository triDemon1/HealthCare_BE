using HaNoiTravel.DTOS;

namespace HaNoiTravel.Interfaces
{
    public interface IBookingManagementService
    {
        // Booking Management
        Task<IEnumerable<BookingAdminDto>> GetAllBookingsAsync();
        Task<BookingAdminDto?> GetBookingByIdAsync(int bookingId);
        Task<BookingAdminDto?> UpdateBookingStatusAsync(int bookingId, StatusUpdateDto statusDto); // Ví dụ cập nhật trạng thái
        Task<bool> DeleteBookingAsync(int bookingId);
        Task<BookingAdminDto?> UpdateBookingAsync(int bookingId, BookingUpdateDto bookingDto);
    }
}
