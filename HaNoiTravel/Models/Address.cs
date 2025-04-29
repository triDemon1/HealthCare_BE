using System;
using System.Collections.Generic;

namespace HaNoiTravel.Models;

public partial class Address
{
    public int Addressid { get; set; }

    public int Customerid { get; set; }

    public string? Country { get; set; }

    public string? Street { get; set; }

    public string? Ward { get; set; }

    public string? District { get; set; }

    public string? City { get; set; }

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    public virtual Customer Customer { get; set; } = null!;

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
