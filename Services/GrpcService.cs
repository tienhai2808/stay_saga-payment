using PaymentGrpc = Grpc.Payment.PaymentService;
using PaymentService.Repositories;
using Grpc.Payment;
using Grpc.Core;
using System.Globalization;

namespace PaymentService.Services;

public sealed class GrpcService(PaymentRepository paymentRepo) : PaymentGrpc.PaymentServiceBase
{
    private readonly PaymentRepository _paymentRepo = paymentRepo;

    public override Task<EmptyResponse> Ping(PingRequest request, ServerCallContext context)
    {
        return Task.FromResult(new EmptyResponse());
    }

    public override async Task<BasicPaymentsResponse> ListBasicPaymentByKeycloakIdAndBookingId(
        ListBasicPaymentByKeycloakIdAndBookingIdRequest request,
        ServerCallContext context
    )
    {
        var payments = await _paymentRepo.FindAllByKeycloakIdAndBookingIdAsync(
            request.BookingId,
            request.KeycloakId,
            context.CancellationToken
        );

        var response = new BasicPaymentsResponse();
        response.Payments.AddRange(payments.Select(payment =>
        {
            var basicPayment = new BasicPaymentResponse
            {
                Id = payment.Id,
                Amount = (float)payment.Amount,
                Status = payment.Status
            };

            if (!string.IsNullOrWhiteSpace(payment.Provider))
                basicPayment.Provider = payment.Provider;

            if (!string.IsNullOrWhiteSpace(payment.Method))
                basicPayment.Method = payment.Method;

            if (payment.PaidAt.HasValue)
                basicPayment.PaidAt = payment.PaidAt?.ToString("yyyy-MM-dd HH:mm:ss");

            if (payment.FailedAt.HasValue)
                basicPayment.FailedAt = payment.FailedAt?.ToString("yyyy-MM-dd HH:mm:ss");

            return basicPayment;
        }));

        return response;
    }
}
