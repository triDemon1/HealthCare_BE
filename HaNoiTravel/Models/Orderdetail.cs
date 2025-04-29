using System;
using System.Collections.Generic;

namespace HaNoiTravel.Models;

public partial class Orderdetail
{
    public int Orderdetailid { get; set; }

    public int Productid { get; set; }

    public int Orderid { get; set; }

    public int Quantity { get; set; }

    public decimal Priceatpurchase { get; set; }

    public DateTime? Createdat { get; set; }

    public virtual Order Order { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;
}
