using System;
using System.Collections.Generic;

namespace HaNoiTravel.Models;

public partial class Bookingstatus
{
    public int Statusid { get; set; }

    public string Statusname { get; set; } = null!;

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
