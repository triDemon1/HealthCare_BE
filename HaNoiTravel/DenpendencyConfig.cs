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
            services.AddScoped<ILoginService, LoginService>();
            services.AddScoped<IRegisterService, RegisterService>();
            services.AddScoped<IBookingService, BookingService>();
            services.AddScoped<IUserManagementService, UserManagementService>();
            services.AddScoped<ILookupDataService, LookupDataService>();
            services.AddScoped<IBookingManagementService, BookingManagementService>();
            services.AddScoped<IOrderManagementService, OrderManagementService>();
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<IOrderService, OrderService>();
            services.AddScoped<ICustomerService, CustomerService>();
            services.AddScoped<IPaymentService, PaymentService>();
            services.AddScoped<IMomoService, MomoService>();
            return services;
        }
    }
}
