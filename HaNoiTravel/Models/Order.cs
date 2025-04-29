using System;
using System.Collections.Generic;

namespace HaNoiTravel.Models;

public partial class Order
{
    public int Orderid { get; set; }

    public int Customerid { get; set; }

    public int Orderstatusid { get; set; }

    public int Addressid { get; set; }

    public DateTime? Orderdate { get; set; }

    public decimal Totalamount { get; set; }

    public DateTime? Createdat { get; set; }

    public DateTime? Updatedat { get; set; }

    public virtual Address Address { get; set; } = null!;

    public virtual Customer Customer { get; set; } = null!;

    public virtual ICollection<Orderdetail> Orderdetails { get; set; } = new List<Orderdetail>();

    public virtual Orderstatus Orderstatus { get; set; } = null!;

    public virtual ICollection<TransactionItem> TransactionItems { get; set; } = new List<TransactionItem>();
}
