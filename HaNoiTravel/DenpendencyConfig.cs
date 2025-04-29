using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using HaNoiTravel.Services; 
using HaNoiTravel.Interfaces; 
namespace HaNoiTravel
{
    public static class DenpendencyConfig
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            // 'this IServiceCollection services' cho phép gọi phương thức này trên đối tượng IServiceCollection
            // 'configuration' được truyền vào để có thể truy cập cấu hình nếu dịch vụ cần

            // Đăng ký AuthService với IAuthService
            services.AddScoped<ILoginService, LoginService>();
            services.AddScoped<IRegisterService, RegisterService>();
            services.AddScoped<IBookingService, BookingService>();

            // Đăng ký AppDbContext (giữ nguyên hoặc chuyển từ Program.cs sang đây)
            // Nếu bạn muốn giữ đăng ký DbContext ở đây:
            // services.AddDbContext<AppDbContext>(options =>
            //     options.UseSqlServer(configuration.GetConnectionString("YourConnectionString")));
            // Lưu ý: Nếu chuyển DbContext sang đây, hãy xóa nó khỏi Program.cs

            // Đăng ký các dịch vụ khác của ứng dụng ở đây
            // services.AddScoped<IOtherService, OtherService>();
            // services.AddTransient<ITransientService, TransientService>();
            // services.AddSingleton<ISingletonService, SingletonService>();

            // Trả về IServiceCollection để cho phép chaining (gọi nhiều phương thức .Add... liên tiếp)
            return services;
        }
    }
}
