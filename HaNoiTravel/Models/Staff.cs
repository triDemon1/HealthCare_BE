using System;
using System.Collections.Generic;

namespace HaNoiTravel.Models;

public partial class Staff
{
    public int Staffid { get; set; }

    public int Userid { get; set; }

    public string Firstname { get; set; } = null!;

    public string Lastname { get; set; } = null!;

    public string? Phonenumber { get; set; }

    public string? Skills { get; set; }

    public int? Expyear { get; set; }

    public bool? Isavailable { get; set; }

    public DateTime? Createat { get; set; }

    public DateTime? Updatedat { get; set; }

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    public virtual User User { get; set; } = null!;
}
