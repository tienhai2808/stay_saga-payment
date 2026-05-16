using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaymentDomainService = PaymentService.Services.PaymentService;

namespace PaymentService.Controllers;

[ApiController]
[Route("payments")]
[Authorize]
public class PaymentController(PaymentDomainService paymentService) : ControllerBase
{
    private readonly PaymentDomainService _paymentService = paymentService;
}