using HaNoiTravel.DTOS;
using HaNoiTravel.Interfaces;
using HaNoiTravel.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace HaNoiTravel.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly ILoginService _loginService; // Sử dụng interface Service
        private readonly IRegisterService _registerService; // Sử dụng interface Service
        private readonly IConfiguration _configuration;

        // Inject IAuthService thông qua constructor
        public AuthController(ILoginService loginService, IRegisterService registerService, IConfiguration configuration)
        {
            _loginService = loginService;
            _registerService = registerService;
            _configuration = configuration;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest) // Đổi tên tham số cho rõ ràng
        {
            try
            {
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

                // *** Gọi phương thức LoginAsync từ Service CHỈ MỘT LẦN ***
                // Giả định LoginAsync trả về LoginServiceResult (hoặc null nếu thất bại)
                var loginResult = await _loginService.LoginAsync(loginRequest, ipAddress);

                // Kiểm tra kết quả trả về từ Service
                if (loginResult == null)
                {
                    // Nếu Service trả về null, tức là đăng nhập thất bại (sai thông tin)
                    return Unauthorized("Invalid email or password."); // Trả về lỗi 401
                }

                // *** Xử lý thành công: Đặt Access Token và Refresh Token vào HttpOnly cookie ***

                // Lấy tên cookie từ cấu hình
                var accessTokenCookieName = _configuration["Cookie:Name"] ?? "jwtToken";
                var refreshTokenCookieName = _configuration["Cookie:RefreshTokenName"] ?? "refreshToken";

                // Lấy thời gian hết hạn từ cấu hình
                var accessTokenExpiresInMinutes = _configuration.GetValue<int>("Jwt:AccessTokenExpiresInMinutes", 15);
                var refreshTokenExpiresInDays = _configuration.GetValue<int>("Jwt:RefreshTokenExpiresInDays", 7);

                // Cấu hình Cookie cho Access Token (ngắn hạn)
                var accessTokenCookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Expires = DateTimeOffset.Now.AddMinutes(accessTokenExpiresInMinutes), // Thời gian hết hạn Access Token
                    Secure = _configuration.GetValue<bool>("Cookie:Secure", true),
                    SameSite = SameSiteMode.Strict, // Nên dùng Strict hoặc Lax
                    IsEssential = true,
                    Domain = _configuration["Cookie:Domain"], // Có thể null nếu cùng domain
                    Path = "/" // Áp dụng cho toàn bộ path của domain
                };

                // Cấu hình Cookie cho Refresh Token (dài hạn)
                var refreshTokenCookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Expires = DateTimeOffset.Now.AddDays(refreshTokenExpiresInDays), // Thời gian hết hạn Refresh Token
                    Secure = _configuration.GetValue<bool>("Cookie:Secure", true),
                    SameSite = SameSiteMode.Strict, // Có thể dùng Lax hoặc Strict tùy yêu cầu bảo mật
                    IsEssential = true,
                    Domain = _configuration["Cookie:Domain"], // Có thể null nếu cùng domain
                                                              // Chỉ gửi Refresh Token đến endpoint refresh để tăng bảo mật (optional nhưng tốt)
                    Path = "/"
                };

                // *** Đặt Access Token vào cookie (MỘT LẦN) ***
                Response.Cookies.Append(accessTokenCookieName, loginResult.AccessToken, accessTokenCookieOptions);

                // *** Đặt Refresh Token vào cookie (MỘT LẦN) ***
                Response.Cookies.Append(refreshTokenCookieName, loginResult.RefreshToken, refreshTokenCookieOptions);


                string accessToken = loginResult.AccessToken; // Access Token
                string refreshTokenString = loginResult.RefreshToken; // Refresh Token string
                string role = loginResult.Role; // Role
                int userID = loginResult.UserId;
                string userName = loginResult.UserName;
                int? Customerid = loginResult.CustomerId;
                return Ok(new { Userid = userID,  Username = userName, Role = role, Customerid = Customerid });
            }
            catch (InvalidOperationException ex)
            {
                // Xử lý lỗi cấu hình JWT Key từ Service
                Console.WriteLine($"JWT Configuration Error: {ex.Message}");
                return StatusCode(500, "Server configuration error.");
            }
            catch (Exception ex)
            {
                // Log lỗi và trả về response 500 cho các lỗi không mong muốn khác
                Console.WriteLine($"An error occurred during login: {ex.ToString()}");
                return StatusCode(500, "Internal Server Error");
            }
        }
        // *** Endpoint Refresh Token ***
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken()
        {
            // Lấy Refresh Token từ cookie
            var refreshTokenCookieName = _configuration["Cookie:RefreshTokenName"] ?? "refreshToken";
            var refreshToken = Request.Cookies[refreshTokenCookieName];

            // Kiểm tra xem có Refresh Token trong cookie không
            if (string.IsNullOrEmpty(refreshToken))
            {
                // Không có Refresh Token - yêu cầu đăng nhập lại
                return Unauthorized("Refresh Token not found.");
            }

            // Lấy địa chỉ IP của client
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

            // Gọi Service để xác thực Refresh Token và sinh token mới
            var result = await _loginService.ValidateRefreshTokenAndGenerateNewTokensAsync(refreshToken, ipAddress);

            if (result == null)
            {
                // Refresh Token không hợp lệ (đã hết hạn, bị thu hồi, không tồn tại)
                // Xóa cookie chứa token cũ (nếu có)
                Response.Cookies.Delete(_configuration["Cookie:Name"] ?? "jwtToken"); // Xóa Access Token cookie
                Response.Cookies.Delete(refreshTokenCookieName); // Xóa Refresh Token cookie

                // Trả về 401 Unauthorized
                return Unauthorized("Invalid or expired Refresh Token.");
            }

            // Refresh Token hợp lệ - Lấy token mới từ kết quả
            string newAccessToken = result.Item1;
            RefreshToken newRefreshToken = result.Item2; // Đối tượng RefreshToken mới
            string role = result.Item3; // Role

            // Lấy tên cookie từ cấu hình
            var accessTokenCookieName = _configuration["Cookie:Name"] ?? "jwtToken";
            // Refresh Token Name đã lấy ở trên

            // Lấy thời gian hết hạn của Refresh Token mới từ đối tượng newRefreshToken
            var refreshTokenExpires = newRefreshToken.ExpiresAt;


            // Cấu hình Cookie cho Access Token mới (ngắn hạn)
            var accessTokenCookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = DateTimeOffset.Now.AddMinutes(_configuration.GetValue<int>("Jwt:AccessTokenExpiresInMinutes", 15)), // Thời gian hết hạn Access Token
                Secure = _configuration.GetValue<bool>("Cookie:Secure", true),
                SameSite = SameSiteMode.Strict,
                IsEssential = true,
                Domain = _configuration["Cookie:Domain"],
                Path = "/"
            };

            // Cấu hình Cookie cho Refresh Token mới (dài hạn)
            var refreshTokenCookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = refreshTokenExpires, // Thời gian hết hạn Refresh Token mới
                Secure = _configuration.GetValue<bool>("Cookie:Secure", true),
                SameSite = SameSiteMode.Strict, // Có thể dùng Lax hoặc Strict tùy yêu cầu bảo mật
                IsEssential = true,
                Domain = _configuration["Cookie:Domain"],
                Path = "/api/auth/refresh-token" // Chỉ gửi Refresh Token đến endpoint refresh
            };

            // Đặt Access Token mới vào cookie
            Response.Cookies.Append(accessTokenCookieName, newAccessToken, accessTokenCookieOptions);

            // Đặt Refresh Token mới vào cookie
            Response.Cookies.Append(refreshTokenCookieName, newRefreshToken.Token, refreshTokenCookieOptions);

            // Trả về phản hồi thành công (không cần body hoặc chỉ trả về Role)
            return Ok(new { Role = role });
        }

        // *** Endpoint Logout ***
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            // Lấy Refresh Token từ cookie (để thu hồi nó ở database)
            var refreshTokenCookieName = _configuration["Cookie:RefreshTokenName"] ?? "refreshToken";
            var refreshToken = Request.Cookies[refreshTokenCookieName];

            // Lấy địa chỉ IP của client
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

            // Nếu có Refresh Token, gọi Service để thu hồi nó
            if (!string.IsNullOrEmpty(refreshToken))
            {
                await _loginService.RevokeRefreshTokenAsync(refreshToken, ipAddress, "User initiated logout");
            }

            // Xóa cả Access Token và Refresh Token cookie khỏi trình duyệt
            Response.Cookies.Delete(_configuration["Cookie:Name"] ?? "jwtToken"); // Xóa Access Token cookie
            Response.Cookies.Delete(refreshTokenCookieName); // Xóa Refresh Token cookie

            // Trả về phản hồi thành công
            return Ok("Logged out successfully.");
        }

        // Helper để lấy địa chỉ IP của client (có thể cần điều chỉnh tùy môi trường triển khai)
        private string GetIpAddress()
        {
            // HttpContext.Connection.RemoteIpAddress chỉ hoạt động tốt khi chạy cục bộ hoặc không qua proxy/load balancer
            // Trong môi trường production, cần lấy IP từ header như "X-Forwarded-For"
            if (Request.Headers.ContainsKey("X-Forwarded-For"))
                return Request.Headers["X-Forwarded-For"];
            else
                return HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString(); // MapToIPv4 để lấy địa chỉ IPv4 nếu có
        }
        // *** Endpoint Đăng ký ***
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest registerRequest)
        {
            // Kiểm tra ModelState trước khi gọi Service
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // Trả về lỗi validation từ Data Annotations trong DTO
            }

            try
            {
                var result = await _registerService.RegisterAsync(registerRequest);

                if (result.Success)
                {
                    // Đăng ký thành công, trả về response 200 OK hoặc 201 Created
                    // Có thể trả về thông tin user đã đăng ký (không bao gồm mật khẩu băm)
                    return Ok(result); // Hoặc Created(...)
                }
                else
                {
                    // Đăng ký thất bại, trả về Conflict (409) với thông báo lỗi cụ thể từ Service
                    return Conflict(result);
                }
            }
            // Có thể bắt các loại Exception cụ thể hơn nếu Service throw chúng
            // catch (InvalidOperationException ex) // Ví dụ nếu Service throw khi trùng lặp
            // {
            //     return Conflict(ex.Message);
            // }
            catch (Exception ex)
            {
                // Log lỗi và trả về response 500 cho các lỗi không mong muốn khác
                Console.WriteLine($"An error occurred during registration: {ex.ToString()}");
                return StatusCode(500, "Internal Server Error");
            }
        }
    }
}
