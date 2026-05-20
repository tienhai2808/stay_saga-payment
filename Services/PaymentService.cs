using System.Globalization;
using PayOS;
using PayOS.Models.V2.PaymentRequests;
using PayOS.Models.Webhooks;
using PaymentService.DTOs;
using PaymentService.Models;
using PaymentService.Repositories;
using Common.Exceptions;
using PaymentService.Configs;

namespace PaymentService.Services;

public class PaymentService(
    PayOSClient payOsClient,
    PayOSConfigs payOsConfigs,
    PaymentRepository paymentRepo
)
{
    private readonly PayOSClient _payOsClient = payOsClient;
    private readonly PayOSConfigs _payOsConfigs = payOsConfigs;
    private readonly PaymentRepository _paymentRepo = paymentRepo;

    public async Task<ProcessPaymentResponseDto> ProcessPaymentAsync(
        string keycloakId,
        ProcessPaymentRequestDto request,
        CancellationToken cancellationToken = default
    )
    {
        if (!long.TryParse(request.BookingId, out var bookingId))
            throw new BadRequestException("Invalid booking id.");

        var payment = await _paymentRepo.FindByBookingIdAndKeycloakIdAsync(
            bookingId,
            keycloakId,
            cancellationToken
        ) ?? throw new NotFoundException("Payment not found for this booking.");

        if (payment.Status == PaymentStatuses.Paid)
            throw new BadRequestException("Booking has already been paid.");

        var amount = Convert.ToInt64(Math.Round(payment.Amount, MidpointRounding.AwayFromZero));
        if (amount <= 0)
            throw new BadRequestException("Payment amount must be greater than zero.");
        if (payment.OrderCode <= 0)
            throw new InternalServerException("Payment order code is invalid.");

        var paymentRequest = new CreatePaymentLinkRequest
        {
            OrderCode = payment.OrderCode,
            Amount = amount,
            Description = BuildDescription(payment.BookingId),
            ReturnUrl = _payOsConfigs.ReturnUrl,
            CancelUrl = _payOsConfigs.CancelUrl,
            Items =
            [
                new PaymentLinkItem
                {
                    Name = $"Booking {payment.BookingId}",
                    Quantity = 1,
                    Price = amount,
                    Unit = "order"
                }
            ]
        };

        var paymentLink = await _payOsClient.PaymentRequests.CreateAsync(paymentRequest);

        payment.Provider = PaymentProviders.PayOS;
        payment.Method = PaymentMethods.Qr;
        payment.TransactionId = paymentLink.PaymentLinkId;
        payment.Status = PaymentStatuses.Pending;
        await _paymentRepo.UpdateAsync(payment, cancellationToken);

        return new ProcessPaymentResponseDto
        {
            PaymentId = payment.Id,
            BookingId = payment.BookingId,
            OrderCode = payment.OrderCode,
            PaymentLinkId = paymentLink.PaymentLinkId ?? string.Empty,
            CheckoutUrl = paymentLink.CheckoutUrl ?? string.Empty,
            QrCode = paymentLink.QrCode ?? string.Empty,
            Provider = payment.Provider ?? string.Empty,
            Method = payment.Method ?? string.Empty,
            Status = payment.Status
        };
    }

    public async Task HandlePayOsWebhookAsync(Webhook webhook, CancellationToken cancellationToken = default)
    {
        var verifiedData = await _payOsClient.Webhooks.VerifyAsync(webhook);

        var payment = await _paymentRepo.FindByOrderCodeAsync(verifiedData.OrderCode, cancellationToken)
            ?? throw new NotFoundException("Payment not found.");

        var isPaymentSuccess = string.Equals(verifiedData.Code, "00", StringComparison.Ordinal)
            || string.Equals(webhook.Code, "00", StringComparison.Ordinal)
            || webhook.Success;

        if (isPaymentSuccess)
        {
            payment.Status = PaymentStatuses.Paid;
            payment.Provider = PaymentProviders.PayOS;
            payment.Method = PaymentMethods.Qr;
            payment.TransactionId = !string.IsNullOrWhiteSpace(verifiedData.Reference)
                ? verifiedData.Reference
                : verifiedData.PaymentLinkId;
            payment.PaidAt = ParsePayOsDateTime(verifiedData.TransactionDateTime);
        }
        else
        {
            payment.Status = PaymentStatuses.Failed;
            payment.FailedAt = DateTime.UtcNow;
        }

        await _paymentRepo.UpdateAsync(payment, cancellationToken);
    }

    private static string BuildDescription(long bookingId)
    {
        var description = $"BK{bookingId}";
        return description.Length > 25 ? description[..25] : description;
    }

    private static DateTime ParsePayOsDateTime(string? rawDateTime)
    {
        if (string.IsNullOrWhiteSpace(rawDateTime))
            return DateTime.UtcNow;

        if (DateTimeOffset.TryParse(rawDateTime, out var dto))
            return dto.UtcDateTime;

        if (DateTime.TryParseExact(
            rawDateTime,
            "yyyy-MM-dd HH:mm:ss",
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out var parsed
        ))
            return DateTime.SpecifyKind(parsed, DateTimeKind.Utc);

        return DateTime.UtcNow;
    }
}
