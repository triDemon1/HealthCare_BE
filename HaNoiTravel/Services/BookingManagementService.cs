using HaNoiTravel.Data;
using HaNoiTravel.DTOS;
using HaNoiTravel.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HaNoiTravel.Services
{
    public class BookingManagementService : IBookingManagementService
    {
        private readonly AppDbContext _context;
        public BookingManagementService(AppDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<BookingAdminDto>> GetAllBookingsAsync()
        {
            var bookings = await _context.Bookings
                .Include(b => b.Customer)
                .Include(b => b.Service)
                .Include(b => b.Subject)
                .Include(b => b.Status)
                .Include(b => b.Staff) // Include Staff
                .Include(b => b.Address) // Include Address
                .Include(b => b.PaymentStatus)
                .OrderByDescending(b => b.Scheduledstarttime)
                .Select(b => new BookingAdminDto
                {
                    BookingId = b.Bookingid,
                    ServiceName = b.Service.Name,
                    ServiceId = b.Serviceid,
                    SubjectId = b.Subjectid,
                    SubjectName = b.Subject.Name,
                    StatusId = b.Statusid,
                    StatusName = b.Status.Statusname,
                    ScheduledStartTime = b.Scheduledstarttime,
                    ScheduledEndTime = b.Scheduledstarttime,
                    paymentStatusName = b.PaymentStatus.Statusname,
                    PriceAtBooking = b.Priceatbooking,
                    Notes = b.Notes,
                    CreatedAt = b.Createdat,
                    CustomerId = b.Customerid,
                    CustomerName = $"{b.Customer.Firstname} {b.Customer.Lastname}".Trim(), // Combine Customer Name
                    StaffId = b.Staffid,
                    StaffName = b.Staff != null ? $"{b.Staff.Firstname} {b.Status.Statusname}".Trim() : null, // Combine Staff Name if Staff exists
                    AddressId = b.Addressid,
                    AddressStreet = b.Address.Street,
                    AddressWard = b.Address.Ward,
                    AddressDistrict = b.Address.District,
                    AddressCity = b.Address.City,
                    AddressCountry = b.Address.Country
                })
                .ToListAsync();

            return bookings;
        }

        public async Task<BookingAdminDto?> GetBookingByIdAsync(int bookingId)
        {
            var booking = await _context.Bookings
               .Include(b => b.Customer)
               .Include(b => b.Service)
               .Include(b => b.Subject)
               .Include(b => b.Status)
               .Include(b => b.Staff)
               .Include(b => b.Address)
               .Include(b => b.PaymentStatus)
               .Where(b => b.Bookingid == bookingId)
               .Select(b => new BookingAdminDto 
               {
                   BookingId = b.Bookingid,
                   ServiceName = b.Service.Name,
                   ServiceId = b.Serviceid,
                   SubjectId = b.Subjectid,
                   SubjectName = b.Subject.Name,
                   StatusId = b.Statusid,
                   StatusName = b.Status.Statusname,
                   ScheduledStartTime = b.Scheduledstarttime,
                   ScheduledEndTime = b.Scheduledendtime,
                   PaymentStatusId = b.PaymentStatus.Paymentstatusid,
                   paymentStatusName = b.PaymentStatus.Statusname,
                   PriceAtBooking = b.Priceatbooking,
                   Notes = b.Notes,
                   CreatedAt = b.Createdat,
                   CustomerId = b.Customerid,
                   CustomerName = $"{b.Customer.Firstname} {b.Customer.Lastname}".Trim(),
                   StaffId = b.Staffid,
                   StaffName = b.Staff != null ? $"{b.Staff.Firstname} {b.Staff.Lastname}".Trim() : null,
                   AddressId = b.Addressid,
                   AddressStreet = b.Address.Street,
                   AddressWard = b.Address.Ward,
                   AddressDistrict = b.Address.District,
                   AddressCity = b.Address.City,
                   AddressCountry = b.Address.Country
               })
               .FirstOrDefaultAsync();

            return booking;
        }
        // Implementation for updating booking status using StatusUpdateDto
        public async Task<BookingAdminDto?> UpdateBookingStatusAsync(int bookingId, StatusUpdateDto statusDto)
        {
            var booking = await _context.Bookings
                                      .FirstOrDefaultAsync(b => b.Bookingid == bookingId);

            if (booking == null)
            {
                return null; // Booking not found
            }

            // Validate if the new StatusId exists
            var statusExists = await _context.Bookingstatuses.AnyAsync(bs => bs.Statusid == statusDto.StatusId);
            if (!statusExists)
            {
                // Throw a specific exception or return a result indicating invalid status
                throw new ArgumentException($"Booking status with ID {statusDto.StatusId} not found.");
            }

            booking.Statusid = statusDto.StatusId;
            // Update notes if provided in the DTO
            if (statusDto.Notes != null)
            {
                booking.Notes = statusDto.Notes;
            }
            booking.Updatedat = DateTime.Now; // Assuming you have an UpdatedAt field in Booking model

            await _context.SaveChangesAsync();

            // Fetch and return the updated booking as DTO with relations
            return await GetBookingByIdAsync(bookingId); // Reuse GetBookingByIdAsync
        }

        // Implementation for deleting a booking
        public async Task<bool> DeleteBookingAsync(int bookingId)
        {
            var booking = await _context.Bookings
                                     // Include related entities if they need to be cascade deleted manually
                                     // .Include(b => b.RelatedEntity)
                                     .FirstOrDefaultAsync(b => b.Bookingid == bookingId);

            if (booking == null)
            {
                return false; // Booking not found
            }

            // If you have CASCADE DELETE configured in your database schema or EF Core model,
            // deleting the Booking record will automatically delete related records (e.g., BookingItems if any).
            // If not, you need to delete them explicitly here before deleting the Booking.
            // Example (if no CASCADE DELETE):
            // var bookingItems = await _context.BookingItems.Where(bi => bi.BookingId == bookingId).ToListAsync();
            // _context.BookingItems.RemoveRange(bookingItems);

            _context.Bookings.Remove(booking);
            var result = await _context.SaveChangesAsync();

            // SaveChangesAsync returns the number of state entries written to the database.
            // If result > 0, at least one entity (the booking) was deleted.
            return result > 0;
        }
        public async Task<BookingAdminDto?> UpdateBookingAsync(int bookingId, BookingUpdateDto bookingDto)
        {
            var booking = await _context.Bookings
                                      .FirstOrDefaultAsync(b => b.Bookingid == bookingId);

            if (booking == null)
            {
                return null; // Booking not found
            }

            // --- Validation (Optional but Recommended) ---
            // Validate if the new ServiceId exists
            var serviceExists = await _context.Services.AnyAsync(s => s.Serviceid == bookingDto.ServiceId);
            if (!serviceExists)
            {
                throw new ArgumentException($"Service with ID {bookingDto.ServiceId} not found.");
            }

            // Validate if the new StaffId exists (if not null)
            if (bookingDto.StaffId.HasValue)
            {
                var staffExists = await _context.Staff.AnyAsync(s => s.Staffid == bookingDto.StaffId.Value);
                if (!staffExists)
                {
                    throw new ArgumentException($"Staff with ID {bookingDto.StaffId.Value} not found.");
                }
            }

            // Validate if the new AddressId exists and belongs to the customer of THIS booking
            // We need the original booking's CustomerId for this check
            var originalBooking = await _context.Bookings.AsNoTracking().FirstOrDefaultAsync(b => b.Bookingid == bookingId);
            if (originalBooking == null) // Should not happen if booking was found above, but good practice
            {
                return null;
            }

            var addressExistsForCustomer = await _context.Addresses
                                                         .AnyAsync(a => a.Addressid == bookingDto.AddressId && a.Customerid == originalBooking.Customerid);
            if (!addressExistsForCustomer)
            {
                throw new ArgumentException($"Address with ID {bookingDto.AddressId} not found or does not belong to the booking's customer.");
            }

            // Validate if the new StatusId exists
            var statusExists = await _context.Bookingstatuses.AnyAsync(bs => bs.Statusid == bookingDto.StatusId);
            if (!statusExists)
            {
                throw new ArgumentException($"Booking status with ID {bookingDto.StatusId} not found.");
            }

            // Validate date/time logic (e.g., start time before end time, not in the past for certain statuses)
            if (bookingDto.ScheduledStartTime >= bookingDto.ScheduledEndTime)
            {
                throw new ArgumentException("Scheduled start time must be before scheduled end time.");
            }
            // --- End Validation ---


            // --- Update Booking Properties ---
            booking.Serviceid = bookingDto.ServiceId;
            booking.Staffid = bookingDto.StaffId;
            booking.Scheduledstarttime = bookingDto.ScheduledStartTime;
            booking.Scheduledendtime = bookingDto.ScheduledEndTime;
            booking.Priceatbooking = bookingDto.PriceAtBooking;
            booking.Addressid = bookingDto.AddressId;
            booking.Statusid = bookingDto.StatusId;
            booking.PaymentStatusId = bookingDto.PaymentStatusId;
            booking.Notes = bookingDto.Notes;
            booking.Updatedat = DateTime.Now; // Assuming you have an UpdatedAt field

            await _context.SaveChangesAsync();

            // Fetch and return the updated booking as DTO with relations
            return await GetBookingByIdAsync(bookingId); // Reuse GetBookingByIdAsync
        }
    }
}
