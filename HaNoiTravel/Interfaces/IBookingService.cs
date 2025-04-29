using HaNoiTravel.Models;
using HaNoiTravel.DTOS;
namespace HaNoiTravel.Interfaces
{
    public interface IBookingService
    {
        Task<IEnumerable<Subjecttype>> GetSubjectTypesAsync();
        Task<IEnumerable<Service>> GetAllServicesAsync();
        Task<IEnumerable<Service>> GetServicesBySubjectTypeAsync(int subjectTypeId);
        Task<IEnumerable<Address>> GetCustomerAddressesAsync(int customerId);
        Task<IEnumerable<Subject>> GetExistingSubjectsAsync(int customerId, int typeId);
        Task<bool> CreateBookingAsync(BookingPayload payload); // Return bool or a Booking object
    }
}
