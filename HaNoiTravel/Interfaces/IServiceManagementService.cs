using HaNoiTravel.DTOS;
using HaNoiTravel.Models;

namespace HaNoiTravel.Interfaces
{
    public interface IServiceManagementService
    {
        Task<Pagination<Service>> GetAllServicesAsync(int pageIndex, int pageSize);
        Task<IEnumerable<Servicegroup>> GetServicegroupAsync();
        Task<ServiceDto?> GetServiceByIdAsync(int id);
        Task<ServiceDto> AddServiceAsync(ServiceCreateDto service);
        Task<bool> UpdateServiceAsync(ServiceUpdateDto service); // Thay đổi kiểu trả về thành bool
        Task<bool> DeleteServiceAsync(int id);
        Task<IEnumerable<Servicegroup>> GetServiceGroupsBySubjectTypeAsync(int subjectTypeId);
    }
}
