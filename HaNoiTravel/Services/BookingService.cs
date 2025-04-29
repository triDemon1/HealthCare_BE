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

            if (payload.SubjectId.HasValue && payload.SubjectId > 0)
            {
                // Case 1: Existing SubjectId is provided
                // Optional: Validate if the SubjectId exists and belongs to the customer
                var existingSubject = await _context.Subjects
                                                    .FirstOrDefaultAsync(s => s.Subjectid == payload.SubjectId.Value && s.Customerid == payload.CustomerId);
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
                if (payload.NewSubjectData.TypeId <= 0)
                {
                    // Handle error: TypeId is required for a new subject
                    return false; // Or throw an exception
                }

                var newSubject = new Subject
                {
                    Customerid = payload.CustomerId,
                    Typeid = payload.NewSubjectData.TypeId,
                    Name = payload.NewSubjectData.Name,
                    Dateofbirth = payload.NewSubjectData.DateOfBirth,
                    Gender = payload.NewSubjectData.Gender,
                    Medicalnotes = payload.NewSubjectData.MedicalNotes,
                    Imageurl = payload.NewSubjectData.ImageUrl,
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
                Addressid = payload.AddressId,
                Subjectid = subjectIdToUse, // Assign the non-nullable subjectIdToUse
                // Handle StaffId if applicable (can be null)
                Staffid = payload.StaffId > 0 ? payload.StaffId : null,
                Statusid = pendingStatus.Statusid, // Assign the found status ID
                Customerid = payload.CustomerId,
                Serviceid = payload.ServiceId,
                Priceatbooking = payload.PriceAtBooking, // Assuming price is sent or calculated here
                Scheduledstarttime = payload.ScheduledStartTime,
                Scheduledendtime = payload.ScheduledEndTime,
                Notes = payload.Notes,
                Createdat = System.DateTime.UtcNow // Set creation time
            };


            _context.Bookings.Add(booking);
            var result = await _context.SaveChangesAsync();

            return result > 0; // Return true if at least one entity was saved (the booking)
                               // Note: SaveChangesAsync() was already called for new Subject if applicable.
        }
    }
}
