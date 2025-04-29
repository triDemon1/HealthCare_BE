using System;
using System.Collections.Generic;

namespace HaNoiTravel.Models;

public partial class Paymentmethod
{
    public int Paymentmethodid { get; set; }

    public string Methodname { get; set; } = null!;

    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}
