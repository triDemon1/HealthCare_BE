using System;
using System.Collections.Generic;

namespace HaNoiTravel.Models;

public partial class Orderstatus
{
    public int Orderstatusid { get; set; }

    public string Statusname { get; set; } = null!;

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
