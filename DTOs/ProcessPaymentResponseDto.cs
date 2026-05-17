namespace PaymentService.DTOs;

public class ProcessPaymentResponseDto
{
    public long PaymentId { get; set; }
    public long BookingId { get; set; }
    public long OrderCode { get; set; }
    public string PaymentLinkId { get; set; } = string.Empty;
    public string CheckoutUrl { get; set; } = string.Empty;
    public string QrCode { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}
