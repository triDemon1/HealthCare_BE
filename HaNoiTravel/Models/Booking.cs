using System;
using System.Collections.Generic;

namespace HaNoiTravel.Models;

public partial class Booking
{
    public int Bookingid { get; set; }

    public int Addressid { get; set; }

    public int Subjectid { get; set; }

    public int? Staffid { get; set; }

    public int Statusid { get; set; }

    public int Customerid { get; set; }

    public int Serviceid { get; set; }

    public decimal Priceatbooking { get; set; }

    public DateTime Scheduledstarttime { get; set; }

    public DateTime Scheduledendtime { get; set; }

    public DateTime? Actualstarttime { get; set; }

    public DateTime? Actualendtime { get; set; }

    public string? Notes { get; set; }

    public DateTime? Createdat { get; set; }

    public DateTime? Updatedat { get; set; }

    public int? PaymentStatusId { get; set; }

    public virtual Address Address { get; set; } = null!;

    public virtual Customer Customer { get; set; } = null!;

    public virtual Paymentstatus? PaymentStatus { get; set; }

    public virtual Service Service { get; set; } = null!;

    public virtual Staff? Staff { get; set; }

    public virtual Bookingstatus Status { get; set; } = null!;

    public virtual Subject Subject { get; set; } = null!;

    public virtual ICollection<TransactionItem> TransactionItems { get; set; } = new List<TransactionItem>();
}
