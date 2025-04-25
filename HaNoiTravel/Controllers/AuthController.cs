using HaNoiTravel.DTOS;
using HaNoiTravel.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
        // Fake user list
        private List<FakeUser> users = new List<FakeUser>
            {
                new FakeUser { Username = "admin", Password = "123456", Role = "admin" },
                new FakeUser { Username = "user", Password = "123456", Role = "user" }
            };

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest login)
        {
            try
            {
                var user = users.FirstOrDefault(u => u.Username == login.Email && u.Password == login.Password);
                if (user == null)
                    return Unauthorized();

                var token = GenerateJwtToken(user);
                return Ok(new { token, user.Role });
            }
            catch (Exception ex)
            {
                // Log lỗi và trả về response 500
                Console.WriteLine(ex.ToString());
                return StatusCode(500, "Internal Server Error");
            }
            
        }
        [HttpGet("login-test")]
        public IActionResult LoginTest()
        {
            return Ok("API is working!");
        }

        private string GenerateJwtToken(FakeUser user)
        {
            // Key phải có ít nhất 256 bits (32 bytes)
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("your_256_bits_super_secret_key_1234567890"));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.Username),  
                new Claim(ClaimTypes.Role, user.Role)
            };

            var token = new JwtSecurityToken(
                issuer: "your-app",
                audience: "your-app",
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
