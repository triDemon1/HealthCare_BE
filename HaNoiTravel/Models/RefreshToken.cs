using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace HaNoiTravel.Models;

public partial class RefreshToken
{
    public int Id { get; set; }

    public string Token { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime ExpiresAt { get; set; }

    public DateTime? RevokedAt { get; set; }

    public string? CreatedByIp { get; set; }

    public string? RevokedByIp { get; set; }

    public string? ReplacedByToken { get; set; }

    public int UserId { get; set; }

    public virtual User User { get; set; } = null!;
    [NotMapped] // <--- Thuộc tính này không ánh xạ vào cột trong DB
    public bool IsActive => RevokedAt == null && !IsExpired;

    // Phương thức kiểm tra token đã hết hạn chưa
    [NotMapped]
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
}
