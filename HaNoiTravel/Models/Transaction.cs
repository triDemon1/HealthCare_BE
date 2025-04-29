using System;
using System.Collections.Generic;

namespace HaNoiTravel.Models;

public partial class Transaction
{
    public int TransactionId { get; set; }

    public int CustomerId { get; set; }

    public DateTime? TransactionDate { get; set; }

    public decimal TotalAmount { get; set; }

    public int PaymentMethodId { get; set; }

    public int PaymentStatusId { get; set; }

    public string? TransactionIdPaymentGateway { get; set; }

    public DateTime? Createdat { get; set; }

    public DateTime? Updatedat { get; set; }

    public virtual Customer Customer { get; set; } = null!;

    public virtual Paymentmethod PaymentMethod { get; set; } = null!;

    public virtual Paymentstatus PaymentStatus { get; set; } = null!;

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual ICollection<TransactionItem> TransactionItems { get; set; } = new List<TransactionItem>();
}
