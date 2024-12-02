using Microsoft.EntityFrameworkCore;

namespace SharedLibrary.TransactionalOutbox;

public static class TransactionalOutboxExtensions
{
    public static void UseTransactionalOutbox(this ModelBuilder builder)
    {
        builder.Entity<OutboxEntity>(builder =>
        {
            builder.ToTable("transactional_outbox");

            builder.HasKey(e => e.Id);
        });
    }
}
