using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Common.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PayOS.Models.Webhooks;
using PaymentService.DTOs;
using PaymentDomainService = PaymentService.Services.PaymentService;
using Common.DTOs;

namespace PaymentService.Controllers;

[ApiController]
[Route("payments")]
public class PaymentController(PaymentDomainService paymentService) : ControllerBase
{
    private readonly PaymentDomainService _paymentService = paymentService;

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> ProcessPayment(
        ProcessPaymentRequestDto dto,
        CancellationToken cancellationToken
    )
    {
        var keycloakId = GetCurrentKeycloakId();
        var paymentRes = await _paymentService.ProcessPaymentAsync(keycloakId, dto, cancellationToken);
        var response = HttpApiResponseDto<ProcessPaymentResponseDto>.Success(
            paymentRes,
            "Payment completed successfully"
        );

        return Ok(response);
    }

    [HttpPost("webhooks/payos")]
    public async Task<IActionResult> ProcessPayOsWebhook(
        Webhook webhook,
        CancellationToken cancellationToken
    )
    {
        await _paymentService.HandlePayOsWebhookAsync(webhook, cancellationToken);
        var response = HttpApiResponseDto<object>.Success(
            "Webhook processed successfully"
        );
        return Ok(response);
    }

    private string GetCurrentKeycloakId()
    {
        var keycloakId = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? throw new UnauthorizedException("Invalid access token");
        return keycloakId;
    }
}
