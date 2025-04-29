using System;
using System.Collections.Generic;

namespace HaNoiTravel.Models;

public partial class Category
{
    public int Categoryid { get; set; }

    public string Name { get; set; } = null!;

    public int? Parentcatoregoryid { get; set; }

    public string? Description { get; set; }

    public DateTime? Createdat { get; set; }

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
