using IdGen;
using PaymentService.Repositories;

namespace PaymentService.Services;

public class PaymentService(
  IIdGenerator<long> idGenerator,
  PaymentRepository paymentRepo
)
{
    private readonly IIdGenerator<long> _idGenerator = idGenerator;
    private readonly PaymentRepository _paymentRepo = paymentRepo;
}