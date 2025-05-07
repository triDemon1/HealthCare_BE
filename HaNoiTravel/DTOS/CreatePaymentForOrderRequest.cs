namespace HaNoiTravel.DTOS
{
    public class CreatePaymentForOrderRequest
    {
        public int OrderId { get; set; }
        public string ReturnUrl { get; set; }
    }
}
