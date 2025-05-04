namespace HaNoiTravel.Interfaces
{
    public interface ICustomerService
    {
        Task<int?> GetCustomerIdByUserIdAsync(int userId);
        Task<bool> ValidateAddressOwnershipAsync(int addressId, int customerId);
    }
}
