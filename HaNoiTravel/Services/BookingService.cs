using HaNoiTravel.DTOS;
using HaNoiTravel.Interfaces;
using HaNoiTravel.Data;
using HaNoiTravel.Models;
using Microsoft.EntityFrameworkCore; // Cần cho FirstOrDefaultAsync
using Microsoft.Extensions.Configuration; // Cần để đọc cấu hình JWT Key
using Microsoft.IdentityModel.Tokens; // Cần cho SymmetricSecurityKey, SigningCredentials
using System;
using System.IdentityModel.Tokens.Jwt; // Cần cho JwtSecurityToken
using System.Security.Claims; // Cần cho ClaimTypes
using System.Text; // Cần cho Encoding.UTF8
using System.Threading.Tasks;
namespace HaNoiTravel.Services
{
    public class BookingService: IBookingService
    {
        private readonly AppDbContext _context;

        public BookingService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Subjecttype>> GetSubjectTypesAsync()
        {
            return await _context.Subjecttypes.ToListAsync();
        }

        public async Task<IEnumerable<Service>> GetAllServicesAsync()
        {
            return await _context.Services.ToListAsync();
        }
        public async Task<IEnumerable<Paymentstatus>> GetPaymentStatusesAsync()
        {
            return await _context.Paymentstatuses.ToListAsync();
        }
        public async Task<IEnumerable<Service>> GetServicesBySubjectTypeAsync(int subjectTypeId)
        {
            return await _context.Services
                                 .Where(s => s.Subjecttypeid == subjectTypeId)
                                 .ToListAsync();
        }

        public async Task<IEnumerable<Address>> GetCustomerAddressesAsync(int customerId)
        {
            return await _context.Addresses
                                 .Where(a => a.Customerid == customerId)
                                 .ToListAsync();
        }

        public async Task<IEnumerable<Subject>> GetExistingSubjectsAsync(int customerId, int typeId)
        {
            return await _context.Subjects
                                 .Where(s => s.Customerid == customerId && s.Typeid == typeId)
                                 .ToListAsync();
        }

        public async Task<bool> CreateBookingAsync(BookingPayload payload)
        {
            // Basic validation (you should add more robust validation)
            if (payload == null)
            {
                return false; // Or throw an exception, depending on error handling strategy
            }

            // --- Logic to handle SubjectId or NewSubjectData ---
            int subjectIdToUse;

            if (payload.subjectId.HasValue && payload.subjectId > 0)
            {
                // Case 1: Existing SubjectId is provided
                // Optional: Validate if the SubjectId exists and belongs to the customer
                var existingSubject = await _context.Subjects
                                                    .FirstOrDefaultAsync(s => s.Subjectid == payload.subjectId.Value && s.Customerid == payload.customerId);
                if (existingSubject == null)
                {
                    // Handle error: Invalid SubjectId provided
                    return false; // Or throw an exception
                }
                subjectIdToUse = existingSubject.Subjectid;
            }
            else if (payload.NewSubjectData != null)
            {
                // Case 2: New Subject data is provided
                // Optional: Add validation for NewSubjectData
                if (payload.NewSubjectData.typeId <= 0)
                {
                    // Handle error: TypeId is required for a new subject
                    return false; // Or throw an exception
                }

                var newSubject = new Subject
                {
                    Customerid = payload.customerId,
                    Typeid = payload.NewSubjectData.typeId,
                    Name = payload.NewSubjectData.name,
                    Dateofbirth = payload.NewSubjectData.dateOfBirth,
                    Gender = payload.NewSubjectData.gender,
                    Medicalnotes = payload.NewSubjectData.medicalNotes,
                    Imageurl = payload.NewSubjectData.imageUrl,
                    Createdat = System.DateTime.UtcNow // Set creation time
                };

                _context.Subjects.Add(newSubject);
                await _context.SaveChangesAsync(); // Save to get the new SubjectId

                subjectIdToUse = newSubject.Subjectid; // Use the newly generated ID
            }
            else
            {
                // Handle error: Neither SubjectId nor NewSubjectData was provided
                return false; // Or throw an exception
            }
            // --- End of Subject handling logic ---


            // Get the StatusId for "Pending"
            var pendingStatus = await _context.Bookingstatuses
                                              .FirstOrDefaultAsync(bs => bs.Statusname == "Pending");

            if (pendingStatus == null)
            {
                // Handle error: "Pending" status not found in DB
                return false; // Or throw a specific exception
            }


            // Map payload to Booking entity using the determined subjectIdToUse
            var booking = new Booking
            {
                Addressid = payload.addressId,
                Subjectid = subjectIdToUse, // Assign the non-nullable subjectIdToUse
                // Handle StaffId if applicable (can be null)
                Staffid = payload.staffId > 0 ? payload.staffId : null,
                Statusid = pendingStatus.Statusid, // Assign the found status ID
                Customerid = payload.customerId,
                Serviceid = payload.serviceId,
                Priceatbooking = payload.priceAtBooking, // Assuming price is sent or calculated here
                Scheduledstarttime = payload.scheduledStartTime,
                Scheduledendtime = payload.scheduledEndTime,
                Notes = payload.notes,
                Createdat = System.DateTime.UtcNow // Set creation time
            };


            _context.Bookings.Add(booking);
            var result = await _context.SaveChangesAsync();

            return result > 0; // Return true if at least one entity was saved (the booking)
                               // Note: SaveChangesAsync() was already called for new Subject if applicable.
        }
        public async Task<Pagination<BookingResponse>> GetCustomerBookingsAsync(
             int customerId,
             int pageIndex,
             int pageSize,
             string? searchTerm = null)
        {
            var query = _context.Bookings
                .Where(b => b.Customerid == customerId)
                .Include(b => b.Service)
                .Include(b => b.Subject)
                .Include(b => b.Status)
                .Include(b => b.PaymentStatus)
                .AsQueryable();

            // Áp dụng bộ lọc tìm kiếm nếu có searchTerm
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(b => b.Subject != null && b.Subject.Name.Contains(searchTerm));
            }

            query = query.OrderByDescending(b => b.Scheduledstarttime);

            var totalCount = await query.CountAsync();

            var bookings = await query
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .Select(b => new BookingResponse
                {
                    BookingId = b.Bookingid,
                    ServiceId = b.Serviceid,
                    ServiceName = b.Service.Name,
                    SubjectId = b.Subjectid,
                    SubjectName = b.Subject.Name,
                    StatusId = b.Statusid,
                    StatusName = b.Status.Statusname,
                    ScheduledStartTime = b.Scheduledstarttime,
                    ScheduledEndTime = b.Scheduledendtime,
                    PriceAtBooking = b.Priceatbooking,
                    Notes = b.Notes,
                    CreatedAt = b.Createdat,
                    PaymentStatusId = b.PaymentStatus != null ? b.PaymentStatus.Paymentstatusid : null,
                    paymentStatusName = b.PaymentStatus != null ? b.PaymentStatus.Statusname : "Chưa thanh toán"
                })
                .ToListAsync();

            return new Pagination<BookingResponse>
            {
                Items = bookings,
                TotalCount = totalCount,
                PageIndex = pageIndex,
                PageSize = pageSize
            };
        }
        public async Task<bool> CancelBookingAsync(int bookingId, int customerId)
        {
            // Tìm booking cần hủy, bao gồm trạng thái booking và trạng thái thanh toán
            var booking = await _context.Bookings
                                        .Include(b => b.Status)
                                        .Include(b => b.PaymentStatus)
                                        .FirstOrDefaultAsync(b => b.Bookingid == bookingId && b.Customerid == customerId);

            // Kiểm tra xem booking có tồn tại, thuộc về khách hàng này
            // VÀ trạng thái booking KHÔNG phải là "Cancelled" hoặc "Completed"
            // VÀ trạng thái thanh toán KHÔNG phải là "Completed"
            if (booking == null ||
                booking.Status.Statusname == "Cancelled" ||
                booking.Status.Statusname == "Completed" ||
                (booking.PaymentStatus != null && booking.PaymentStatus.Statusname == "Completed")) // Kiểm tra trạng thái thanh toán
            {
                // Log lý do không hủy được để debug (tùy chọn)
                if (booking == null) Console.WriteLine($"Booking {bookingId} not found for customer {customerId}.");
                else if (booking.Status.Statusname == "Cancelled") Console.WriteLine($"Booking {bookingId} is already cancelled.");
                else if (booking.Status.Statusname == "Completed") Console.WriteLine($"Booking {bookingId} is already completed.");
                else if (booking.PaymentStatus != null && booking.PaymentStatus.Statusname == "Completed") Console.WriteLine($"Booking {bookingId} is already paid.");

                return false; // Không thể hủy booking
            }

            // Tìm ID của trạng thái "Cancelled" trong BookingStatus
            var cancelledBookingStatus = await _context.Bookingstatuses
                                                     .FirstOrDefaultAsync(bs => bs.Statusname == "Cancelled");

            // Tìm ID của trạng thái "Cancelled" trong PaymentStatus
            var cancelledPaymentStatus = await _context.Paymentstatuses
                                                     .FirstOrDefaultAsync(ps => ps.Statusname == "Cancelled");


            if (cancelledBookingStatus == null || cancelledPaymentStatus == null)
            {
                // Xử lý lỗi: Không tìm thấy trạng thái "Cancelled" trong DB
                Console.Error.WriteLine("Status 'Cancelled' not found in database for BookingStatus or PaymentStatus.");
                return false;
            }

            // Cập nhật trạng thái booking và trạng thái thanh toán
            booking.Statusid = cancelledBookingStatus.Statusid;
            booking.PaymentStatusId = cancelledPaymentStatus.Paymentstatusid;
            booking.Updatedat = System.DateTime.Now;

            var result = await _context.SaveChangesAsync();

            return result > 0;
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
    }
}
