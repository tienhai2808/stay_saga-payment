using Microsoft.EntityFrameworkCore;
using PaymentService.Models;

namespace PaymentService.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Payment> Payments => Set<Payment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var payment = modelBuilder.Entity<Payment>();
        payment.ToTable(
            "payments",
            table =>
            {
                table.HasCheckConstraint(
                    "ck_payments_status",
                    $"status in ('{PaymentStatuses.Pending}', '{PaymentStatuses.Paid}', '{PaymentStatuses.Failed}', '{PaymentStatuses.Refunded}')"
                );
                table.HasCheckConstraint(
                    "ck_payments_provider",
                    $"provider IN ('{PaymentProviders.PayOS}', '{PaymentProviders.MoMo}', '{PaymentProviders.VnPay}')"
                );
                table.HasCheckConstraint(
                    "ck_payments_method",
                    $"method IN ('{PaymentMethods.Qr}', '{PaymentMethods.Card}', '{PaymentMethods.Wallet}')"
                );
            }
        );

        payment.HasKey(x => x.Id);
        payment.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        payment.Property(x => x.KeycloakId)
            .HasColumnName("keycloak_id")
            .IsRequired()
            .HasMaxLength(64)
            .HasColumnType("character varying(64)");

        payment.Property(x => x.BookingId)
            .HasColumnName("booking_id")
            .IsRequired();
        
        payment.Property(x => x.Amount)
            .HasColumnName("amount")
            .HasPrecision(18, 2)
            .IsRequired();

        payment.Property(x => x.Status)
            .HasColumnName("status")
            .IsRequired()
            .HasMaxLength(20)
            .HasColumnType("character varying(20)");

        payment.Property(x => x.Method)
            .HasColumnName("method")
            .HasMaxLength(20)
            .HasColumnType("character varying(20)");

        payment.Property(x => x.Provider)
            .HasColumnName("provider")
            .HasMaxLength(20)
            .HasColumnType("character varying(20)");

        payment.Property(x => x.TransactionId)
            .HasColumnName("transaction_id")
            .HasColumnType("character varying(150)");

        payment.Property(x => x.PaidAt)
            .HasColumnName("paid_at")
            .HasColumnType("timestamp with time zone");

        payment.Property(x => x.FailedAt)
            .HasColumnName("failed_at")
            .HasColumnType("timestamp with time zone");
    }
}