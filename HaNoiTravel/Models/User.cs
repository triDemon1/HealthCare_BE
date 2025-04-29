using System;
using System.Collections.Generic;

namespace HaNoiTravel.Models;

public partial class User
{
    public int Userid { get; set; }

    public string Username { get; set; } = null!;

    public string Passwordhash { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? Phonenumber { get; set; }

    public DateTime? Createdat { get; set; }

    public DateTime? Updatedat { get; set; }

    public bool Isactive { get; set; }

    public int Roleid { get; set; }

    public virtual Customer? Customer { get; set; }

    public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

    public virtual Role Role { get; set; } = null!;

    public virtual ICollection<Staff> Staff { get; set; } = new List<Staff>();
}
