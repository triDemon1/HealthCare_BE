using System;
using System.Collections.Generic;

namespace HaNoiTravel.Models;

public partial class Customer
{
    public int Customerid { get; set; }

    public int Userid { get; set; }

    public string Firstname { get; set; } = null!;

    public string? Lastname { get; set; }

    public DateOnly? Dateofbirth { get; set; }

    public bool? Gender { get; set; }

    public DateTime? Createdat { get; set; }

    public DateTime? Updatedat { get; set; }

    public virtual ICollection<Address> Addresses { get; set; } = new List<Address>();

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<Subject> Subjects { get; set; } = new List<Subject>();

    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();

    public virtual User User { get; set; } = null!;
}
