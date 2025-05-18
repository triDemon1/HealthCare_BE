using HaNoiTravel.Data;
using HaNoiTravel.DTOS;
using HaNoiTravel.Interfaces;
using HaNoiTravel.Models;
using Microsoft.EntityFrameworkCore;

namespace HaNoiTravel.Services
{
    public class ServiceManagementService:IServiceManagementService
    {
        private readonly AppDbContext _context;
        public ServiceManagementService(AppDbContext context) // Constructor injection
        {
            _context = context;
        }

        // Modified method to support pagination
        public async Task<Pagination<Service>> GetAllServicesAsync(int pageIndex, int pageSize)
        {
            // Ensure pageIndex and pageSize are valid
            if (pageIndex < 0) pageIndex = 0;
            if (pageSize <= 0) pageSize = 10; // Default or minimum page size

            var query = _context.Services.AsQueryable();

            int totalCount = await query.CountAsync(); // Get total count before pagination

            var items = await query
                .OrderBy(s => s.Serviceid) // Order by a consistent column for predictable pagination
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new Pagination<Service>
            {
                Items = items,
                TotalCount = totalCount,
                PageIndex = pageIndex,
                PageSize = pageSize
            };
        }
        public async Task<IEnumerable<Servicegroup>> GetServicegroupAsync()
        {
            return await _context.Servicegroups.ToListAsync();
        }
        public async Task<ServiceDto?> GetServiceByIdAsync(int id)
        {
            var s = await _context.Services
         .Include(s => s.Servicegroup)
         .Include(s => s.Subjecttype)
         .FirstOrDefaultAsync(s => s.Serviceid == id);

            if (s == null) return null;

            return new ServiceDto
            {
                Serviceid = s.Serviceid,
                Name = s.Name,
                Description = s.Description,
                Duration = s.Duration,
                Price = s.Price,
                Isactive = s.Isactive,
                Servicegroupid = s.Servicegroupid,
                ServicegroupName = s.Servicegroup?.Name,
                Subjecttypeid = s.Subjecttypeid,
                SubjecttypeName = s.Subjecttype?.Subjectname
            };
        }

        public async Task<ServiceDto> AddServiceAsync(ServiceCreateDto dto)
        {
            var service = new Service
            {
                Name = dto.Name,
                Description = dto.Description,
                Duration = dto.Duration,
                Price = dto.Price,
                Isactive = dto.Isactive,
                Servicegroupid = dto.Servicegroupid,
                Subjecttypeid = dto.Subjecttypeid,
                Createdat = DateTime.Now
            };

            _context.Services.Add(service);
            await _context.SaveChangesAsync();

            return await GetServiceByIdAsync(service.Serviceid) ?? throw new Exception("Lỗi tạo dịch vụ");
        }

        public async Task<bool> UpdateServiceAsync(ServiceUpdateDto dto)
        {
            var existing = await _context.Services.FindAsync(dto.Serviceid);
            if (existing == null) return false;

            existing.Name = dto.Name;
            existing.Description = dto.Description;
            existing.Duration = dto.Duration;
            existing.Price = dto.Price;
            existing.Isactive = dto.Isactive;
            existing.Servicegroupid = dto.Servicegroupid;
            existing.Subjecttypeid = dto.Subjecttypeid;
            existing.Updatedat = DateTime.Now;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteServiceAsync(int id)
        {
            var serviceToDelete = await _context.Services.FindAsync(id);
            if (serviceToDelete == null)
            {
                return false; // Không tìm thấy dịch vụ để xóa
            }

            _context.Services.Remove(serviceToDelete);
            await _context.SaveChangesAsync();
            return true; // Xóa thành công
        }
        public async Task<IEnumerable<Servicegroup>> GetServiceGroupsBySubjectTypeAsync(int subjectTypeId)
        {
            // This query joins Services and Servicegroups
            // Filters by the given subjectTypeId from the Services table
            // Selects the distinct Servicegroup associated with those services
            // Returns a list of Servicegroup objects
            var serviceGroups = await _context.Services
                .Where(s => s.Subjecttypeid == subjectTypeId)
                .Select(s => s.Servicegroup)
                .Distinct() // Ensures you get unique service groups
                .ToListAsync();

            return serviceGroups;
        }

    }
}
