namespace PaymentService.Models;

public class Payment
{
    public long Id { get; set; }
    public int OrderCode { get; set; }
    public long BookingId { get; set; }
    public string KeycloakId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? Provider { get; set; }
    public string Status { get; set; } = PaymentStatuses.Pending;
    public string? TransactionId { get; set; }
    public string? Method { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime? FailedAt { get; set; }
}
