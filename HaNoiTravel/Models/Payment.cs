using System;
using System.Collections.Generic;

namespace HaNoiTravel.Models;

public partial class Payment
{
    public int Paymentid { get; set; }

    public decimal Amount { get; set; }

    public DateTime? Paymentdate { get; set; }

    public int Transactionid { get; set; }

    public string? TransactionidPaymentGateway { get; set; }

    public DateTime? Createdat { get; set; }

    public DateTime? Updatedat { get; set; }

    public virtual Transaction Transaction { get; set; } = null!;
}
