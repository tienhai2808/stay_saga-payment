using Common.Constants;
using Common.Events;
using DotNetCore.CAP;
using IdGen;
using PaymentService.Models;
using PaymentService.Repositories;

namespace PaymentService.Consumers;

public class BookingCreatedConsumer(
    IIdGenerator<long> idGenerator,
    PaymentRepository paymentRepo
) : ICapSubscribe
{
    private readonly IIdGenerator<long> _idGenerator = idGenerator;
    private readonly PaymentRepository _paymentRepo = paymentRepo;

    [CapSubscribe(TopicConstants.BookingCreatedTopic)]
    public async Task HandleAsync(BookingCreatedEvent mess, CancellationToken cancellationToken = default)
    {
        var payment = new Payment
        {
            Id = _idGenerator.CreateId(),
            BookingId = mess.BookingId,
            KeycloakId = mess.KeycloakId,
            Amount = mess.Amount,
            Status = PaymentStatuses.Pending,
        };
        await _paymentRepo.CreateAsync(payment, cancellationToken);
    }
}