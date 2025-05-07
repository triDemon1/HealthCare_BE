using System;
using System.Collections.Generic;

namespace HaNoiTravel.Models;

public partial class Paymentstatus
{
    public int Paymentstatusid { get; set; }

    public string Statusname { get; set; } = null!;

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}
