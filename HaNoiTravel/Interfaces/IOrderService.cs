using HaNoiTravel.DTOS;
using System.Security.Claims;

namespace HaNoiTravel.Interfaces
{
    public interface IOrderService
    {
        Task<(int? OrderId, string? ErrorMessage)> CreateOrderAsync(CreateOrderDto createOrderDto, ClaimsPrincipal user);
    }
}
