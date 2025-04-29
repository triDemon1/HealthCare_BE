using System;
using System.Collections.Generic;

namespace HaNoiTravel.Models;

public partial class Product
{
    public int Productid { get; set; }

    public int Categoryid { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public decimal Price { get; set; }

    public int Stockquantity { get; set; }

    public string? Imageurl { get; set; }

    public string? Sku { get; set; }

    public bool Isactive { get; set; }

    public DateTime? Createdat { get; set; }

    public DateTime? Updatedat { get; set; }

    public virtual Category Category { get; set; } = null!;

    public virtual ICollection<Orderdetail> Orderdetails { get; set; } = new List<Orderdetail>();
}
