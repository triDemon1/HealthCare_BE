using System;
using System.Collections.Generic;

namespace HaNoiTravel.Models;

public partial class Servicegroup
{
    public int Servicegroupid { get; set; }

    public string? Name { get; set; }

    public virtual ICollection<Service> Services { get; set; } = new List<Service>();
}
