using System.ComponentModel.DataAnnotations;

namespace PaymentService.DTOs;

public class ProcessPaymentRequestDto
{
    [Required(ErrorMessage = "Booking id is required.")]
    [RegularExpression("^[0-9]+$", ErrorMessage = "Booking id must contain digits only.")]
    [MaxLength(20, ErrorMessage = "Booking id must be at most 20 characters.")]
    public string BookingId { get; set; } = string.Empty;
}
