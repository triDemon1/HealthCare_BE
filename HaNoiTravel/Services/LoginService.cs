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
        private readonly IEmailService _emailService;

        // Inject AppDbContext và IConfiguration thông qua constructor
        public LoginService(AppDbContext context, IConfiguration configuration, IEmailService emailService)
        {
            _context = context;
            _configuration = configuration;
            _emailService = emailService;
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
        public async Task<bool> RequestPasswordReset(string emailOrPhone)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == emailOrPhone || u.Phonenumber == emailOrPhone);

            if (user == null)
            {
                // Luôn trả về true để tránh lộ thông tin user tồn tại hay không vì lý do bảo mật.
                return true;
            }

            // Tạo mã OTP 6 chữ số
            var otp = GenerateOtp(); // Phương thức mới để tạo OTP
            var expiryTime = DateTime.Now.AddMinutes(1); // OTP hết hạn sau 5 phút (có thể cấu hình)

            // Lưu OTP và thời gian hết hạn vào user
            user.PasswordResetToken = otp;
            user.PasswordResetTokenExpiry = expiryTime;
            user.Updatedat = DateTime.Now; // Cập nhật thời gian update
            await _context.SaveChangesAsync();

            var subject = "Mã xác nhận đặt lại mật khẩu của bạn";
            var message = $"Chào bạn {user.Username},<br/><br/>" +
                          $"Mã xác nhận đặt lại mật khẩu của bạn là: <strong>{otp}</strong><br/><br/>" +
                          $"Mã này sẽ hết hạn trong 5 phút. Vui lòng không chia sẻ mã này với bất kỳ ai.<br/><br/>" +
                          $"Nếu bạn không yêu cầu đặt lại mật khẩu, vui lòng bỏ qua email này.<br/><br/>" +
                          $"Trân trọng,<br/>" +
                          $"Đội ngũ hỗ trợ của bạn";

            await _emailService.SendEmailAsync(user.Email, subject, message);

            return true;
        }

        // Phương thức mới để xác nhận OTP
        public async Task<(bool success, string message, string? tempResetToken)> VerifyPasswordResetOtp(string emailOrPhone, string otp)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => (u.Email == emailOrPhone || u.Phonenumber == emailOrPhone) && u.PasswordResetToken == otp);

            if (user == null)
            {
                return (false, "Mã OTP hoặc email/số điện thoại không hợp lệ.", null);
            }

            if (user.PasswordResetTokenExpiry <= DateTime.UtcNow)
            {
                // Xóa OTP đã hết hạn
                user.PasswordResetToken = null;
                user.PasswordResetTokenExpiry = null;
                await _context.SaveChangesAsync();
                return (false, "Mã OTP đã hết hạn. Vui lòng yêu cầu mã mới.", null);
            }

            // OTP hợp lệ. Tạo một token tạm thời để dùng cho bước đặt lại mật khẩu.
            // Token này sẽ thay thế OTP trong PasswordResetToken.
            var tempResetToken = GenerateResetToken(); // Sử dụng GUID hoặc chuỗi ngẫu nhiên dài hơn
            user.PasswordResetToken = tempResetToken; // Lưu token tạm thời
            user.PasswordResetTokenExpiry = DateTime.UtcNow.AddMinutes(10); // Thời gian sống mới cho token tạm thời (ví dụ 10 phút)
            user.Updatedat = DateTime.Now;
            await _context.SaveChangesAsync();

            return (true, "Mã OTP hợp lệ.", tempResetToken);
        }

        // Cập nhật phương thức ResetPassword để nhận token tạm thời
        public async Task<bool> ResetPassword(string emailOrPhone, string tempResetToken, string newPassword)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => (u.Email == emailOrPhone || u.Phonenumber == emailOrPhone) && u.PasswordResetToken == tempResetToken); // Kiểm tra bằng tempResetToken

            if (user == null)
            {
                // Token không hợp lệ, đã hết hạn, hoặc email/phone không khớp
                return false;
            }

            if (user.PasswordResetTokenExpiry <= DateTime.UtcNow)
            {
                // Token đã hết hạn
                user.PasswordResetToken = null;
                user.PasswordResetTokenExpiry = null;
                user.Updatedat = DateTime.Now;
                await _context.SaveChangesAsync();
                return false;
            }

            // Hash mật khẩu mới
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(newPassword);

            // Cập nhật mật khẩu
            user.Passwordhash = hashedPassword;

            // Vô hiệu hóa token sau khi sử dụng thành công (quan trọng!)
            user.PasswordResetToken = null;
            user.PasswordResetTokenExpiry = null;
            user.Updatedat = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return true;
        }
        public static string GenerateResetToken()
        {
            // Tạo một token dài hơn, khó đoán hơn cho việc reset thực sự
            return Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N"); // Kết hợp 2 GUID để dài hơn
        }
        public static string GenerateOtp()
        {
            // Tạo mã OTP 6 chữ số
            Random random = new Random();
            return random.Next(100000, 999999).ToString();
        }
    }
}
