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
using System.Security.Cryptography;
namespace HaNoiTravel.Services
{
    public class LoginService : ILoginService
    {
        private readonly AppDbContext _context; // Để thao tác với database
        private readonly IConfiguration _configuration; // Để đọc JWT Key từ cấu hình

        // Inject AppDbContext và IConfiguration thông qua constructor
        public LoginService(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // *** Phương thức sinh Refresh Token ***
        private RefreshToken GenerateRefreshToken(User user, string ipAddress)
        {
            // Thời gian hết hạn của Refresh Token (dài hạn)
            var refreshTokenExpiresInDays = _configuration.GetValue<int>("Jwt:RefreshTokenExpiresInDays", 7); // Mặc định 7 ngày

            using (var rngCryptoServiceProvider = new RNGCryptoServiceProvider())
            {
                var randomBytes = new byte[64]; // Độ dài token
                rngCryptoServiceProvider.GetBytes(randomBytes);
                var refreshToken = new RefreshToken
                {
                    Token = Convert.ToBase64String(randomBytes),
                    ExpiresAt = DateTime.Now.AddDays(refreshTokenExpiresInDays), // Thời gian hết hạn Refresh Token
                    CreatedAt = DateTime.Now,
                    CreatedByIp = ipAddress,
                    UserId = user.Userid
                };
                return refreshToken;
            }
        }

        // Triển khai phương thức LoginAsync từ interface
        public async Task<LoginResponse> LoginAsync(LoginRequest loginRequest, string ipAddress)
        {
            // Ví dụ cập nhật logic xác thực mật khẩu với BCrypt:
            var user = await _context.Users
                                    .Include(u => u.Role)
                                    .Include(u => u.RefreshTokens)
                                    .Include(u => u.Customer)
                                    .ThenInclude(c => c.Addresses)
                                    .FirstOrDefaultAsync(u => u.Username == loginRequest.Username); // Hoặc u.EMAIL == loginRequest.Email
            var addressId = user.Customer?.Addresses?.FirstOrDefault()?.Addressid;
            if (user == null)
            {
                return null; // User not found
            }

            // *** Sử dụng BCrypt để xác minh mật khẩu ***
            if (!BCrypt.Net.BCrypt.Verify(loginRequest.Password, user.Passwordhash))
            {
                return null; // Invalid password
            }
            // Xác thực thành công, sinh Access Token và Refresh Token

            // Sinh Access Token (ngắn hạn)
            var accessToken = GenerateJwtToken(user);

            // Sinh Refresh Token (dài hạn)
            var refreshToken = GenerateRefreshToken(user, ipAddress);

            // Lưu Refresh Token vào database
            user.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync();
            return new LoginResponse
            {
                AccessToken = accessToken,
                Role = user.Role?.Rolename ?? user.Roleid.ToString(),
                UserName = user.Username,
                UserId = user.Userid,
                RefreshToken = refreshToken.Token,
                CustomerId = user.Customer?.Customerid,
                AddressId  = addressId
            };
        }

        // Phương thức tạo JWT Token (Private vì chỉ dùng nội bộ trong Service này)
        private string GenerateJwtToken(User user)
        {
            // Lấy JWT Key từ cấu hình (appsettings.json)
            var jwtKey = _configuration["Jwt:Key"];
            if (string.IsNullOrEmpty(jwtKey) || jwtKey.Length < 32) // Key cần ít nhất 256 bits (32 bytes)
            {
                // Xử lý lỗi hoặc throw Exception nếu key không hợp lệ
                throw new InvalidOperationException("JWT Key is not configured correctly or is too short.");
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Lấy Issuer và Audience từ cấu hình (appsettings.json)
            var issuer = _configuration["Jwt:Issuer"];
            var audience = _configuration["Jwt:Audience"];

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Userid.ToString()), // Thêm UserID vào claim
                new Claim(ClaimTypes.Name, user.Username),
                // Thêm Role vào claim. Sử dụng tên Role nếu có, hoặc RoleId
                new Claim(ClaimTypes.Role, user.Role?.Rolename ?? user.Roleid.ToString())
            };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.Now.AddHours(1), // Thời gian hết hạn của token
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        public async Task<Tuple<string, RefreshToken, string>> ValidateRefreshTokenAndGenerateNewTokensAsync(string refreshToken, string ipAddress)
        {
            // Tìm Refresh Token trong database, bao gồm cả User
            var existingRefreshToken = await _context.RefreshTokens
                .Include(rt => rt.User)
                .Include(rt => rt.User.Role) // Include Role để sinh Access Token mới
                .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

            // Kiểm tra Refresh Token có tồn tại không
            if (existingRefreshToken == null)
            {
                // Token không tồn tại - có thể là tấn công hoặc token đã bị xóa
                return null; // Trả về null nếu không hợp lệ
            }

            // Kiểm tra token đã bị thu hồi hoặc hết hạn chưa
            if (!existingRefreshToken.IsActive)
            {
                // Token đã bị thu hồi hoặc hết hạn - thu hồi tất cả token của user đó
                await RevokeAllRefreshTokensForUserAsync(existingRefreshToken.User, ipAddress, $"Attempted use of inactive token: {refreshToken}");
                return null; // Trả về null nếu không hợp lệ
            }

            // Token hợp lệ - Thu hồi token hiện tại và sinh token mới
            existingRefreshToken.RevokedAt = DateTime.Now;
            existingRefreshToken.RevokedByIp = ipAddress;
            existingRefreshToken.ReplacedByToken = GenerateRandomTokenString(); // Sinh token ngẫu nhiên để thay thế (đánh dấu token cũ đã bị thay thế)

            // Sinh Refresh Token mới
            var newRefreshToken = GenerateRefreshToken(existingRefreshToken.User, ipAddress);
            newRefreshToken.ReplacedByToken = existingRefreshToken.Token; // Đánh dấu token mới thay thế token cũ nào

            // Thêm Refresh Token mới vào database
            _context.RefreshTokens.Add(newRefreshToken);

            // Sinh Access Token mới
            var newAccessToken = GenerateJwtToken(existingRefreshToken.User);

            await _context.SaveChangesAsync();

            // Trả về Tuple chứa Access Token mới, Refresh Token mới và Role
            return Tuple.Create(newAccessToken, newRefreshToken, existingRefreshToken.User.Role?.Rolename ?? existingRefreshToken.User.Roleid.ToString());
        }
        // *** Phương thức thu hồi Refresh Token (cho Logout) ***
        public async Task<bool> RevokeRefreshTokenAsync(string refreshToken, string ipAddress, string reason = null)
        {
            var existingRefreshToken = await _context.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == refreshToken);

            if (existingRefreshToken == null || !existingRefreshToken.IsActive)
            {
                // Token không tồn tại hoặc đã bị thu hồi/hết hạn
                return false;
            }

            // Thu hồi token
            existingRefreshToken.RevokedAt = DateTime.Now;
            existingRefreshToken.RevokedByIp = ipAddress;
            existingRefreshToken.ReplacedByToken = reason ?? "Logged out"; // Ghi lý do thu hồi

            await _context.SaveChangesAsync();
            return true;
        }

        // *** Phương thức thu hồi tất cả Refresh Token của một User ***
        private async Task RevokeAllRefreshTokensForUserAsync(User user, string ipAddress, string reason = null)
        {
            var activeRefreshTokens = await _context.RefreshTokens
                .Where(rt => rt.UserId == user.Userid && rt.RevokedAt == null && DateTime.Now < rt.ExpiresAt)
                .ToListAsync();

            foreach (var token in activeRefreshTokens)
            {
                token.RevokedAt = DateTime.Now;
                token.RevokedByIp = ipAddress;
                token.ReplacedByToken = reason ?? "Revoked all tokens";
            }
            await _context.SaveChangesAsync();
        }
        // Helper để sinh chuỗi token ngẫu nhiên
        private string GenerateRandomTokenString()
        {
            using (var rngCryptoServiceProvider = new RNGCryptoServiceProvider())
            {
                var randomBytes = new byte[64];
                rngCryptoServiceProvider.GetBytes(randomBytes);
                return Convert.ToBase64String(randomBytes);
            }
        }
    }
}
