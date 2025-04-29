using HaNoiTravel.DTOS;
using HaNoiTravel.Interfaces;
using HaNoiTravel.Data;
using HaNoiTravel.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration; 
using Microsoft.IdentityModel.Tokens; 
using System;
using System.IdentityModel.Tokens.Jwt; 
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore.Internal;

namespace HaNoiTravel.Services
{
    public class RegisterService : IRegisterService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private const int CUSTOMER_ROLE_ID = 2; // Giả định RoleID cho Customer là 2 dựa trên script SQL

        public RegisterService(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<RegisterResult> RegisterAsync(RegisterRequest registerRequest)
        {
            // 1. Kiểm tra trùng lặp (Username, Email, PhoneNumber)
            // AnyAsync trả về true nếu tìm thấy bất kỳ bản ghi nào khớp điều kiện
            bool usernameExists = await _context.Users.AnyAsync(u => u.Username == registerRequest.Username);
            if (usernameExists)
            {
                // Có thể throw Exception hoặc trả về mã lỗi cụ thể hơn thay vì chỉ false
                // throw new InvalidOperationException("Username already exists.");
                return RegisterResult.Failure("Tên đăng nhập đã tồn tại.");
            }

            bool emailExists = await _context.Users.AnyAsync(u => u.Email == registerRequest.Email);
            if (emailExists)
            {
                // throw new InvalidOperationException("Email already exists.");
                return RegisterResult.Failure("Email đã tồn tại."); // Email đã tồn tại
            }

            // Kiểm tra số điện thoại chỉ nếu nó được cung cấp (không null hoặc rỗng)
            if (!string.IsNullOrEmpty(registerRequest.PhoneNumber))
            {
                bool phoneExists = await _context.Users.AnyAsync(u => u.Phonenumber == registerRequest.PhoneNumber);
                if (phoneExists)
                {
                    // throw new InvalidOperationException("Phone number already exists.");
                    return RegisterResult.Failure("Số điện thoại đã tồn tại."); // Số điện thoại đã tồn tại
                }
                // Cần kiểm tra trùng lặp số điện thoại trong bảng STAFF nữa nếu cột đó UNIQUE ở cả 2 bảng
                bool staffPhoneExists = await _context.Staff.AnyAsync(s => s.Phonenumber == registerRequest.PhoneNumber);
                if (staffPhoneExists) return RegisterResult.Failure("Số điện thoại đã tồn tại.");
            }


            // 2. Băm mật khẩu
            // BCrypt.Net.BCrypt.HashPassword(password_cleartext)
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(registerRequest.Password);

            // 3. Tạo Entity USER
            var newUser = new User
            {
                Username = registerRequest.Username,
                Passwordhash = hashedPassword, // Lưu mật khẩu đã băm
                Email = registerRequest.Email,
                Phonenumber = registerRequest.PhoneNumber,
                Isactive = true,
                Roleid = CUSTOMER_ROLE_ID,
                Createdat = DateTime.UtcNow, 
                Updatedat = DateTime.UtcNow 
            };

            // Thêm User vào DbContext
            _context.Users.Add(newUser);

            // 4. Lưu User để có USERID tự sinh
            // Phải gọi SaveChangesAsync ở đây để có được USERID trước khi tạo Customer
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Xử lý lỗi khi lưu User (ví dụ: lỗi database khác)
                // Log lỗi chi tiết ở đây
                Console.WriteLine($"Error saving User during registration: {ex.ToString()}");
                return RegisterResult.Failure("Lỗi khi lưu thông tin người dùng.");
            }

            // 5. Tạo Entity CUSTOMER
            var newCustomer = new Customer
            {
                Userid = newUser.Userid, // Lấy USERID vừa được sinh ra
                Firstname = registerRequest.FirstName,
                Lastname = registerRequest.LastName,
                Dateofbirth = registerRequest.DateOfBirth,
                Gender = registerRequest.Gender,
                Createdat = DateTime.UtcNow,
                Updatedat = DateTime.UtcNow
            };

            // Thêm Customer vào DbContext
            _context.Customers.Add(newCustomer);

            // 6. Lưu Customer để có CUSTOMERID tự sinh
            // Phải gọi SaveChangesAsync ở đây để có được CUSTOMERID trước khi tạo Address
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Xử lý lỗi khi lưu Customer
                Console.WriteLine($"Error saving Customer during registration: {ex.ToString()}");
                // Cần cân nhắc rollback User đã lưu ở bước 4 nếu lỗi xảy ra ở đây
                // Để đơn giản, tôi chỉ trả về lỗi. Trong thực tế cần Transaction
                return RegisterResult.Failure("Lỗi khi lưu thông tin khách hàng.");
            }

            // 7. Tạo Entity ADDRESSES (Chỉ tạo nếu có ít nhất 1 trường địa chỉ được cung cấp)
            // Bạn có thể điều chỉnh logic này tùy theo yêu cầu: bắt buộc địa chỉ, tùy chọn, v.v.
            bool hasAddressInfo = !string.IsNullOrEmpty(registerRequest.Country) ||
                                  !string.IsNullOrEmpty(registerRequest.Street) ||
                                  !string.IsNullOrEmpty(registerRequest.Ward) ||
                                  !string.IsNullOrEmpty(registerRequest.District) ||
                                  !string.IsNullOrEmpty(registerRequest.City);

            if (hasAddressInfo)
            {
                var newAddress = new Address
                {
                    Customerid = newCustomer.Customerid, // Lấy CUSTOMERID vừa được sinh ra
                    Country = registerRequest.Country,
                    Street = registerRequest.Street,
                    Ward = registerRequest.Ward,
                    District = registerRequest.District,
                    City = registerRequest.City
                    // CREATEDAT, UPDATEDAT không có trong bảng ADDRESSES của bạn
                };
                // Thêm Address vào DbContext
                _context.Addresses.Add(newAddress);

                // 8. Lưu Address
                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    // Xử lý lỗi khi lưu Address
                    Console.WriteLine($"Error saving Address during registration: {ex.ToString()}");
                    // Cần cân nhắc rollback User và Customer đã lưu nếu lỗi xảy ra ở đây
                    // Để đơn giản, tôi chỉ trả về lỗi. Trong thực tế cần Transaction
                    return RegisterResult.Failure("Lỗi khi lưu thông tin địa chỉ.");
                }
            }
            else
            {
                // Nếu không có thông tin địa chỉ, chỉ cần lưu User và Customer đã đủ
                // (Đã lưu ở bước 4 và 6)
            }


            // 9. Nếu mọi thứ thành công
            return RegisterResult.SuccessResult(); // Trả về kết quả thành công
        }
    }
}
