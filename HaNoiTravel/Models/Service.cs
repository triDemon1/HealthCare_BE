using System;
using System.Collections.Generic;

namespace HaNoiTravel.Models;

public partial class Service
{
    public int Serviceid { get; set; }

    public int Servicegroupid { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public int? Duration { get; set; }

    public decimal? Price { get; set; }

    public bool? Isactive { get; set; }

    public DateTime? Createdat { get; set; }

    public DateTime? Updatedat { get; set; }

    public int Subjecttypeid { get; set; }

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    public virtual Servicegroup Servicegroup { get; set; } = null!;

    public virtual Subjecttype Subjecttype { get; set; } = null!;
}
