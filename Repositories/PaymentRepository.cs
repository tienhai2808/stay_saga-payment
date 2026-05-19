using Microsoft.EntityFrameworkCore;
using PaymentService.Data;
using PaymentService.Models;

namespace PaymentService.Repositories;

public class PaymentRepository(AppDbContext db)
{
    private readonly AppDbContext _db = db;

    public async Task CreateAsync(Payment payment, CancellationToken cancellationToken = default)
    {
        _db.Payments.Add(payment);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public Task<Payment?> FindByBookingIdAndKeycloakIdAsync(
        long bookingId,
        string keycloakId,
        CancellationToken cancellationToken = default
    )
        => _db.Payments
            .Where(x => x.BookingId == bookingId && x.KeycloakId == keycloakId)
            .OrderByDescending(x => x.Id)
            .FirstOrDefaultAsync(cancellationToken);

    public Task<Payment?> FindByIdAsync(long id, CancellationToken cancellationToken = default)
        => _db.Payments.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task UpdateAsync(Payment payment, CancellationToken cancellationToken = default)
    {
        _db.Payments.Update(payment);
        await _db.SaveChangesAsync(cancellationToken);
    }
}
