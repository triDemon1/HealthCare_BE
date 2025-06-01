using HaNoiTravel.Data;
using HaNoiTravel.DTOS;
using HaNoiTravel.Models;
using HaNoiTravel.Interfaces;
using Microsoft.EntityFrameworkCore;
namespace HaNoiTravel.Services
{
    public class CustomerService: ICustomerService
    {
        private readonly AppDbContext _context;

        public CustomerService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<int?> GetCustomerIdByUserIdAsync(int userId)
        {
            var customer = await _context.Customers
                                        .AsNoTracking()
                                        .FirstOrDefaultAsync(c => c.Userid == userId);
            return customer?.Customerid; // Trả về CustomerId hoặc null
        }

        public async Task<bool> ValidateAddressOwnershipAsync(int addressId, int customerId)
        {
            return await _context.Addresses
                                 .AnyAsync(a => a.Addressid == addressId && a.Customerid == customerId);
        }
        public async Task<UserProfileDto?> GetUserProfileAsync(int userId)
        {
            var user = await _context.Users
                                   .Include(u => u.Customer) // Include Customer data
                                                             // Không cần Include(u => u.Role) hoặc Include(u => u.Staff) cho profile người dùng cuối
                                   .Where(u => u.Userid == userId)
                                   .Select(u => new UserProfileDto // Sử dụng UserProfileDto mới
                                   {
                                       UserId = u.Userid,
                                       Username = u.Username,
                                       Email = u.Email,
                                       PhoneNumber = u.Phonenumber,
                                       CreatedAt = u.Createdat,
                                       UpdatedAt = u.Updatedat,
                                       // IsActive, RoleId không có trong UserProfileDto


                                       // Map nested Customer DTO if Customer exists
                                       Customer = u.Customer != null ? new CustomerProfileDetailDto // Sử dụng CustomerProfileDetailDto mới
                                       {
                                           CustomerId = u.Customer.Customerid,
                                           FirstName = u.Customer.Firstname,
                                           LastName = u.Customer.Lastname,
                                           DateOfBirth = u.Customer.Dateofbirth,
                                           Gender = u.Customer.Gender,
                                           CreatedAt = u.Customer.Createdat,
                                           UpdatedAt = u.Customer.Updatedat
                                       } : null
                                       // Staff DTO không có trong UserProfileDto
                                   })
                                   .FirstOrDefaultAsync();

            return user;
        }

        // Phương thức cập nhật thông tin profile người dùng hiện tại (đã sửa theo mẫu và DTO mới)
        public async Task<bool> UpdateUserProfileAsync(int userId, UpdateUserProfileDto model)
        {
            // Tìm User và Customer tương ứng
            var user = await _context.Users
                                    .Include(u => u.Customer) // Include Customer để cập nhật
                                                              // Không cần Include(u => u.Staff) hoặc Role cho profile người dùng cuối
                                    .FirstOrDefaultAsync(u => u.Userid == userId);

            if (user == null)
            {
                return false; // Không tìm thấy người dùng
            }

            // --- Cập nhật thông tin User (chỉ những trường được phép) ---
            // Không cho phép cập nhật Username từ form profile người dùng cuối
            // user.Username = model.Username; // Bỏ dòng này

            // Cập nhật Email nếu được cung cấp và khác giá trị hiện tại
            if (!string.IsNullOrEmpty(model.Email) && user.Email != model.Email)
            {
                // Kiểm tra email mới có trùng với người dùng khác không (trừ chính mình)
                var existingUserWithEmail = await _context.Users.AnyAsync(u => u.Email == model.Email && u.Userid != userId);
                if (existingUserWithEmail)
                {
                    // Xử lý lỗi: Email đã tồn tại
                    // Thay vì chỉ trả về false, bạn có thể throw Exception hoặc trả về enum/object lỗi
                    throw new InvalidOperationException("Email đã tồn tại."); // Ví dụ throw Exception
                }
                user.Email = model.Email;
                user.Updatedat = System.DateTime.UtcNow; // Cập nhật thời gian cập nhật User
            }
            // Cập nhật PhoneNumber nếu được cung cấp và khác giá trị hiện tại
            if (!string.IsNullOrEmpty(model.PhoneNumber) && user.Phonenumber != model.PhoneNumber)
            {
                // Kiểm tra phone mới có trùng với người dùng khác không (trừ chính mình)
                var existingUserWithPhone = await _context.Users.AnyAsync(u => u.Phonenumber == model.PhoneNumber && u.Userid != userId);
                if (existingUserWithPhone)
                {
                    // Xử lý lỗi: Số điện thoại đã tồn tại
                    throw new InvalidOperationException("Số điện thoại đã tồn tại."); // Ví dụ throw Exception
                }
                user.Phonenumber = model.PhoneNumber;
                user.Updatedat = System.DateTime.Now; // Cập nhật thời gian cập nhật User
            }

            // --- Cập nhật thông tin Customer (nếu tồn tại và User role là Customer) ---
            // Giả định user role Customer luôn có Customer record
            if (user.Customer != null)
            {
                // Cập nhật chỉ những trường có trong UpdateUserProfileDto và thuộc Customer
                if (!string.IsNullOrEmpty(model.FirstName)) user.Customer.Firstname = model.FirstName; // FirstName là bắt buộc theo schema
                user.Customer.Lastname = model.LastName;
                user.Customer.Dateofbirth = model.DateOfBirth;
                user.Customer.Gender = model.Gender;
                user.Customer.Updatedat = System.DateTime.UtcNow; // Cập nhật thời gian cập nhật Customer
                                                                  // Không xóa Customer record nếu model.FirstName null hoặc CustomerData null
            }
            else
            {
                // Xử lý trường hợp User role Customer không có Customer profile (không mong muốn)
                // Có thể throw Exception hoặc log cảnh báo
                Console.WriteLine($"Error: User {userId} with role Customer does not have a linked Customer profile.");
                // Tùy chọn: Nếu muốn tự động tạo Customer profile nếu chưa có
                // if (!string.IsNullOrEmpty(model.FirstName))
                // {
                //     user.Customer = new Customer
                //     {
                //         Userid = userId,
                //         Firstname = model.FirstName,
                //         LastName = model.LastName,
                //         Dateofbirth = model.DateOfBirth,
                //         Gender = model.Gender,
                //         Createdat = System.DateTime.UtcNow
                //     };
                //     _context.Customers.Add(user.Customer);
                // } else {
                //    // Không thể tạo Customer mới nếu FirstName không được cung cấp
                //    throw new InvalidOperationException("Không thể tạo Customer profile vì thiếu Tên.");
                // }
                // Hiện tại, chúng ta sẽ coi đây là lỗi và không cho phép cập nhật.
                throw new InvalidOperationException("Không tìm thấy Customer profile liên kết.");
            }
            // --- Kết thúc cập nhật thông tin Customer ---

            // Không xử lý StaffData ở đây vì đây là chức năng profile cho người dùng cuối (Customer)
            // if (userDto.StaffData != null) { ... }


            var result = await _context.SaveChangesAsync();

            // Trả về true nếu có ít nhất 1 thay đổi được lưu vào DB
            return result > 0;
            // Lưu ý: Hàm gốc của bạn trả về usersAdminDto sau khi update.
            // Đối với hàm profile người dùng cuối, trả về bool (thành công/thất bại) là đủ,
            // sau đó frontend sẽ tự gọi lại API GetUserProfile để lấy dữ liệu mới nhất.
            // Nếu bạn muốn trả về UserProfileDto sau khi cập nhật, bạn có thể làm:
            // if (result > 0) {
            //    return await GetUserProfileAsync(userId); // Tải lại profile sau khi cập nhật
            // }
            // return null; // Trả về null nếu không có thay đổi hoặc lỗi
        }
        public async Task<Pagination<PurchaseHistoryDto>> GetPurchaseHistoryAsync(int customerId, int pageIndex, int pageSize)
        {
            // Tạo IQueryable cho các mục lịch sử từ Order
            var orderHistoryQuery = _context.TransactionItems
                .AsNoTracking() // Quan trọng: chỉ đọc
                .Where(ti => ti.Transaction.CustomerId == customerId && ti.OrderId != null && ti.Order != null)
                // Sử dụng SelectMany để "làm phẳng" OrderDetails thành các PurchaseHistoryDto
                // Mỗi OrderDetail sẽ trở thành một PurchaseHistoryDto riêng biệt
                .SelectMany(ti => ti.Order.Orderdetails.Select(od => new PurchaseHistoryDto
                {
                    ItemType = "Order",
                    // Ưu tiên TransactionDate từ Transaction, nếu null thì dùng Createdat của Transaction
                    TransactionDate = ti.Transaction.TransactionDate ?? ti.Transaction.Createdat,
                    ItemName = od.Product.Name ?? "Không rõ", // Giả sử Product không bao giờ null nếu OrderDetail tồn tại
                    Amount = od.Priceatpurchase * od.Quantity,
                    Status = ti.Transaction.PaymentStatus.Statusname ?? "Không rõ"
                }));

            // Tạo IQueryable cho các mục lịch sử từ Booking
            var bookingHistoryQuery = _context.TransactionItems
                .AsNoTracking() // Quan trọng: chỉ đọc
                .Where(ti => ti.Transaction.CustomerId == customerId && ti.BookingId != null && ti.Booking != null)
                // Sử dụng Select để chiếu Booking thành PurchaseHistoryDto
                .Select(ti => new PurchaseHistoryDto
                {
                    ItemType = "Booking",
                    TransactionDate = ti.Transaction.TransactionDate ?? ti.Transaction.Createdat,
                    ItemName = ti.Booking.Service.Name ?? "Dịch vụ không rõ", // Giả sử Service không bao giờ null
                    Amount = ti.Booking.Priceatbooking,
                    Status = ti.Transaction.PaymentStatus.Statusname ?? "Không rõ"
                });

            // Kết hợp hai IQueryable bằng Concat.
            // EF Core sẽ cố gắng dịch điều này thành một câu lệnh UNION ALL trong SQL.
            var combinedQuery = orderHistoryQuery.Concat(bookingHistoryQuery);

            // Đếm tổng số mục trên IQueryable kết hợp (sẽ thực hiện COUNT trên DB)
            var totalCount = await combinedQuery.CountAsync();

            // Áp dụng sắp xếp và phân trang trên IQueryable kết hợp.
            // EF Core sẽ cố gắng dịch toàn bộ (UNION ALL, ORDER BY, OFFSET/FETCH) thành một truy vấn SQL.
            var pagedItems = await combinedQuery
                                   .OrderByDescending(p => p.TransactionDate)
                                   .ThenBy(p => p.ItemName) // Sắp xếp phụ cho nhất quán
                                   .Skip(pageIndex * pageSize)
                                   .Take(pageSize)
                                   .ToListAsync();

            return new Pagination<PurchaseHistoryDto>
            {
                Items = pagedItems,
                TotalCount = totalCount,
                PageIndex = pageIndex,
                PageSize = pageSize
            };
        }

    }
}
