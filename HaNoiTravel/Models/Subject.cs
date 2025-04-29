using System;
using System.Collections.Generic;

namespace HaNoiTravel.Models;

public partial class Subject
{
    public int Subjectid { get; set; }

    public int Customerid { get; set; }

    public int Typeid { get; set; }

    public string? Name { get; set; }

    public DateOnly? Dateofbirth { get; set; }

    public bool? Gender { get; set; }

    public string? Medicalnotes { get; set; }

    public DateTime? Createdat { get; set; }

    public DateTime? Updatedat { get; set; }

    public string? Imageurl { get; set; }

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    public virtual Customer Customer { get; set; } = null!;

    public virtual Subjecttype Type { get; set; } = null!;
}
