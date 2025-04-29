using System;
using System.Collections.Generic;

namespace HaNoiTravel.Models;

public partial class TransactionItem
{
    public int TransactionItemId { get; set; }

    public int TransactionId { get; set; }

    public int? OrderId { get; set; }

    public int? BookingId { get; set; }

    public decimal ItemAmount { get; set; }

    public virtual Booking? Booking { get; set; }

    public virtual Order? Order { get; set; }

    public virtual Transaction Transaction { get; set; } = null!;
}
