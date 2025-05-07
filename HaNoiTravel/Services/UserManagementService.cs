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
namespace HaNoiTravel.Services
{
    public class UserManagementService: IUserManagementService
    {
        private readonly AppDbContext _context;
        public UserManagementService(AppDbContext context)
        {
            _context = context;
        }

        
        public async Task<Pagination<usersAdminDto>> GetPagedUsersAsync(int pageIndex, int pageSize, int? RoleId)
        {
            var query = _context.Users // Sử dụng DbSet cho Entity User (tên là USER trong DB schema)
                                .AsQueryable();

            if (RoleId.HasValue)
            {
                query = query.Where(u => u.Roleid == RoleId.Value);
            }

            // Đếm tổng số người dùng khớp với điều kiện lọc
            var totalCount = await query.CountAsync();
            query = query.OrderBy(u => u.Userid);

            // Áp dụng Phân trang (Skip và Take)
            var items = await query
                .Skip(pageIndex * pageSize) // Bỏ qua số mục của các trang trước
                .Take(pageSize)           // Lấy số mục của trang hiện tại
                                          // Bao gồm Role để lấy RoleName cho DTO
                .Include(u => u.Role) // Giả định Navigation Property tên là Role
                .Include(u => u.Customer) // Giả định User có Navigation Property Customer
                .Include(u => u.Staff)
                .Select(u => MapToUserAdminDto(u)) // Chuyển Entity sang DTO
                .ToListAsync();

            // Trả về kết quả trong DTO PagedResult
            return new Pagination<usersAdminDto>
            {
                Items = items,
                TotalCount = totalCount,
                PageIndex = pageIndex,
                PageSize = pageSize
            };
        }
        public async Task<usersAdminDto?> GetUserByIdAsync(int userId)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .Include(u => u.Customer) // Include Customer data
                .Include(u => u.Staff) // Include Staff data
                .Where(u => u.Userid == userId)
                .Select(u => new usersAdminDto
                {
                    UserId = u.Userid,
                    Username = u.Username,
                    Email = u.Email,
                    PhoneNumber = u.Phonenumber,
                    CreatedAt = u.Createdat,
                    UpdatedAt = u.Updatedat,
                    IsActive = u.Isactive,
                    RoleId = u.Roleid,
                    RoleName = u.Role.Rolename,
                    // Map nested Customer DTO if Customer exists
                    Customer = u.Customer != null ? new CustomerAdminDetailDto
                    {
                        CustomerId = u.Customer.Customerid,
                        FirstName = u.Customer.Firstname,
                        LastName = u.Customer.Lastname,
                        DateOfBirth = u.Customer.Dateofbirth,
                        Gender = u.Customer.Gender,
                        CreatedAt = u.Customer.Createdat,
                        UpdatedAt = u.Customer.Updatedat
                    } : null,
                    // Map nested Staff DTO if Staff exists
                    Staff = u.Staff != null ? new StaffAdminDetailDto
                    {
                        StaffId = u.Staff.Staffid,
                        FirstName = u.Staff.Firstname,
                        LastName = u.Staff.Lastname,
                        PhoneNumber = u.Staff.Phonenumber,
                        Skills = u.Staff.Skills,
                        ExpYear = u.Staff.Expyear,
                        IsAvailable = u.Staff.Isavailable,
                        CreateAt = u.Staff.Createat, // Note: Property name is CreateAt
                        UpdatedAt = u.Staff.Updatedat
                    } : null
                })
                .FirstOrDefaultAsync();

            return user;
        }

        public async Task<usersAdminDto> CreateUserAsync(UserCreateUpdateDto userDto)
        {
            // Basic validation (more can be added)
            if (string.IsNullOrEmpty(userDto.Password))
            {
                throw new ArgumentException("Password is required for new user.");
            }

            // Check if username or email already exists
            if (await _context.Users.AnyAsync(u => u.Username == userDto.Username))
            {
                throw new ArgumentException($"Username '{userDto.Username}' already exists.");
            }
            if (await _context.Users.AnyAsync(u => u.Email == userDto.Email))
            {
                throw new ArgumentException($"Email '{userDto.Email}' already exists.");
            }
            if (!string.IsNullOrEmpty(userDto.PhoneNumber) && await _context.Users.AnyAsync(u => u.Phonenumber == userDto.PhoneNumber))
            {
                throw new ArgumentException($"Phone number '{userDto.PhoneNumber}' already exists.");
            }


            var user = new User
            {
                Username = userDto.Username,
                Passwordhash = BCrypt.Net.BCrypt.HashPassword(userDto.Password), // Hash the password
                Email = userDto.Email,
                Phonenumber = userDto.PhoneNumber,
                Isactive = userDto.IsActive,
                Roleid = userDto.RoleId,
                Createdat = DateTime.Now
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync(); // Save to get UserId

            // Create Customer or Staff if data is provided
            if (userDto.CustomerData != null)
            {
                var customer = new Customer
                {
                    Userid = user.Userid,
                    Firstname = userDto.CustomerData.FirstName,
                    Lastname = userDto.CustomerData.LastName,
                    Dateofbirth = userDto.CustomerData.DateOfBirth,
                    Gender = userDto.CustomerData.Gender,
                    Createdat = DateTime.Now
                };
                _context.Customers.Add(customer);
            }

            if (userDto.StaffData != null)
            {
                var staff = new Staff
                {
                    Userid = user.Userid,
                    Firstname = userDto.StaffData.FirstName,
                    Lastname = userDto.StaffData.LastName,
                    Phonenumber = userDto.StaffData.PhoneNumber,
                    Skills = userDto.StaffData.Skills,
                    Expyear = userDto.StaffData.ExpYear,
                    Isavailable = userDto.StaffData.IsAvailable,
                    Createat = DateTime.Now // Note: Property name is CreateAt in your model
                };
                _context.Staff.Add(staff);
            }

            await _context.SaveChangesAsync(); // Save Customer/Staff if added

            // Return the created user as DTO
            return await GetUserByIdAsync(user.Userid); // Fetch the created user with relations
        }

        public async Task<usersAdminDto?> UpdateUserAsync(int userId, UserCreateUpdateDto userDto)
        {
            var user = await _context.Users
                                     .Include(u => u.Customer)
                                     .Include(u => u.Staff)
                                     .FirstOrDefaultAsync(u => u.Userid == userId);

            if (user == null)
            {
                return null; // User not found
            }

            // Update basic user properties
            user.Username = userDto.Username;
            user.Email = userDto.Email;
            user.Phonenumber = userDto.PhoneNumber;
            user.Isactive = userDto.IsActive;
            user.Roleid = userDto.RoleId;
            user.Updatedat = DateTime.Now;

            // Update password only if provided
            if (!string.IsNullOrEmpty(userDto.Password))
            {
                user.Passwordhash = BCrypt.Net.BCrypt.HashPassword(userDto.Password);
            }

            // Handle Customer/Staff data update
            if (userDto.CustomerData != null)
            {
                if (user.Customer == null)
                {
                    // Create new Customer if it doesn't exist
                    user.Customer = new Customer { Userid = user.Userid, Createdat = DateTime.Now };
                    _context.Customers.Add(user.Customer);
                }
                // Update Customer properties
                user.Customer.Firstname = userDto.CustomerData.FirstName;
                user.Customer.Lastname = userDto.CustomerData.LastName;
                user.Customer.Dateofbirth = userDto.CustomerData.DateOfBirth;
                user.Customer.Gender = userDto.CustomerData.Gender;
                user.Customer.Updatedat = DateTime.Now;
            }
            else if (user.Customer != null)
            {
                // If CustomerData is null but Customer exists, you might want to delete the Customer record
                // depending on your business logic. For now, we'll leave it.
                _context.Customers.Remove(user.Customer);
            }


            if (userDto.StaffData != null)
            {
                if (user.Staff == null)
                {
                    // Create new Staff if it doesn't exist
                    user.Staff = new Staff { Userid = user.Userid, Createat = DateTime.Now }; // Note CreateAt
                    _context.Staff.Add(user.Staff);
                }
                // Update Staff properties
                user.Staff.Firstname = userDto.StaffData.FirstName;
                user.Staff.Lastname = userDto.StaffData.LastName;
                user.Staff.Phonenumber = userDto.StaffData.PhoneNumber;
                user.Staff.Skills = userDto.StaffData.Skills;
                user.Staff.Expyear = userDto.StaffData.ExpYear;
                user.Staff.Isavailable = userDto.StaffData.IsAvailable;
                user.Staff.Updatedat = DateTime.UtcNow;
            }
            else if (user.Staff != null)
            {
                // If StaffData is null but Staff exists, you might want to delete the Staff record
                // depending on your business logic. For now, we'll leave it.
                _context.Staff.Remove(user.Staff);
            }


            await _context.SaveChangesAsync();

            // Return the updated user as DTO
            return await GetUserByIdAsync(user.Userid); // Fetch the updated user with relations
        }

        public async Task<bool> DeleteUserAsync(int userId)
        {
            var user = await _context.Users
                                     .Include(u => u.Customer) // Include related entities if you need to delete them
                                     .Include(u => u.Staff)
                                     // Include other related entities with CASCADE DELETE configured in DB or EF Core
                                     // .Include(u => u.RefreshTokens) // Refresh tokens might have cascade delete
                                     .FirstOrDefaultAsync(u => u.Userid == userId);

            if (user == null)
            {
                return false; // User not found
            }

            // Handle deletion of related entities if CASCADE DELETE is not configured in DB/EF Core
            // Example: If a User is deleted, their Customer/Staff record should also be deleted.
            // If you have CASCADE DELETE configured in your database schema or EF Core model,
            // deleting the User record will automatically delete related Customer/Staff/RefreshTokens records.
            // If not, you need to delete them explicitly here before deleting the User.
            // Example (if no CASCADE DELETE):
            if (user.Customer != null) _context.Customers.Remove(user.Customer);
            if (user.Staff != null) _context.Staff.Remove(user.Staff);
            if (user.RefreshTokens != null) _context.RefreshTokens.RemoveRange(user.RefreshTokens);

            _context.Users.Remove(user);
            var result = await _context.SaveChangesAsync();

            return result > 0; // Return true if at least one entity was deleted (the user)
        }
        public async Task<IEnumerable<StaffAdminDetailDto>> GetAllStaffAsync()
        {
            var staffList = await _context.Staff
                .Include(s => s.User) // Include User data to get name etc.
                .Select(s => new StaffAdminDetailDto
                {
                    StaffId = s.Staffid,
                    FirstName = s.Firstname,
                    LastName = s.Lastname,
                    PhoneNumber = s.Phonenumber,
                    Skills = s.Skills,
                    ExpYear = s.Expyear,
                    IsAvailable = s.Isavailable,
                    CreateAt = s.Createat,
                    UpdatedAt = s.Updatedat
                    // You might want to add User details here if needed, e.g., s.User.Username
                })
                .ToListAsync();

            return staffList;
        }
        // Cần đảm bảo Mapping này đúng với User Entity và Role Entity của bạn
        private static usersAdminDto MapToUserAdminDto(User user) // Tham số là Entity USER
        {
            return new usersAdminDto // Trả về DTO UserAdminDto
            {
                UserId = user.Userid, // Ánh xạ từ Entity sang DTO
                Username = user.Username,
                Email = user.Email,
                PhoneNumber = user.Phonenumber, // Sử dụng tên thuộc tính từ Entity
                IsActive = user.Isactive,     // Sử dụng tên thuộc tính từ Entity
                CreatedAt = user.Createdat,   // Sử dụng tên thuộc tính từ Entity (nullable match DateTime?)
                UpdatedAt = user.Updatedat,   // Sử dụng tên thuộc tính từ Entity (nullable match DateTime?)
                RoleId = user.Roleid,         // Lấy RoleId từ Entity User

                // Lấy tên Role từ Navigation Property (cần Include(u => u.Role) trong query)
                // Đảm bảo Navigation Property tên là Role và thuộc tính tên là Rolename
                RoleName = user.Role?.Rolename ?? "Unknown", // Handle null Role if Include fails or Role is null

                // Map nested CustomerAdminDetailDto if Customer navigation property exists and is loaded
                // Cần Include(u => u.Customer) trong query
                CustomerId = user.Customer?.Customerid, // Lấy CustomerId từ Navigation Property Customer
                Customer = user.Customer != null ? MapToCustomerAdminDetailDto(user.Customer) : null, // Map Customer Entity to DTO

                // Map nested StaffAdminDetailDto if Staff navigation property exists and is loaded
                // Cần Include(u => u.Staff) trong query
                StaffId = user.Staff?.Staffid, // Lấy StaffId từ Navigation Property Staff
                Staff = user.Staff != null ? MapToStaffAdminDetailDto(user.Staff) : null // Map Staff Entity to DTO
            };
        }
        private static CustomerAdminDetailDto MapToCustomerAdminDetailDto(Customer customer) // Tham số là Entity Customer
        {
            // Map properties from Customer Entity to CustomerAdminDetailDto
            return new CustomerAdminDetailDto
            {
                CustomerId = customer.Customerid, // Sử dụng tên thuộc tính từ Entity
                FirstName = customer.Firstname,   // Sử dụng tên thuộc tính từ Entity
                LastName = customer.Lastname,     // Sử dụng tên thuộc tính từ Entity
                DateOfBirth = customer.Dateofbirth != null ? customer.Dateofbirth.Value : null, // Map Date to DateOnly (adjust if Entity uses DateOnly)
                Gender = customer.Gender,         // Sử dụng tên thuộc tính từ Entity
                CreatedAt = customer.Createdat,   // Sử dụng tên thuộc tính từ Entity
                UpdatedAt = customer.Updatedat    // Sử dụng tên thuộc tính từ Entity
            };
        }

        // Helper method để map Staff Entity sang StaffAdminDetailDto
        // ** MAKE THIS METHOD STATIC **
        private static StaffAdminDetailDto MapToStaffAdminDetailDto(Staff staff) // Tham số là Entity Staff
        {
            // Map properties from Staff Entity to StaffAdminDetailDto
            return new StaffAdminDetailDto
            {
                StaffId = staff.Staffid,       // Sử dụng tên thuộc tính từ Entity
                FirstName = staff.Firstname,   // Sử dụng tên thuộc tính từ Entity
                LastName = staff.Lastname,     // Sử dụng tên thuộc tính từ Entity
                PhoneNumber = staff.Phonenumber, // Sử dụng tên thuộc tính từ Entity
                Skills = staff.Skills,         // Sử dụng tên thuộc tính từ Entity
                ExpYear = staff.Expyear,       // Sử dụng tên thuộc tính từ Entity
                IsAvailable = staff.Isavailable, // Sử dụng tên thuộc tính từ Entity
                CreateAt = staff.Createat,     // Sử dụng tên thuộc tính từ Entity (Note: CreateAt typo?)
                UpdatedAt = staff.Updatedat    // Sử dụng tên thuộc tính từ Entity
            };
        }
    }
}
