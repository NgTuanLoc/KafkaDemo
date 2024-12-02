using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using SharedLibrary.TransactionalOutbox;
using Newtonsoft.Json;
using SharedLibrary;

namespace Producer;

public sealed class ConvertDomainEventsToOutboxMessagesInterceptor : SaveChangesInterceptor
{
    private readonly JsonSerializerSettings m_SerializerSettings = new()
    {
        TypeNameHandling = TypeNameHandling.All
    };

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        DbContext? dbContext = eventData.Context;

        if (dbContext is null)
        {
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        var outboxMessages = dbContext.ChangeTracker
            .Entries<ProductEntity>()
            .Select(x => x.Entity)
            .SelectMany(baseEntity =>
            {
                var domainEvents = baseEntity.GetEvents();

                baseEntity.ClearEvents();

                return domainEvents;
            })
            .Select(domainEvent => new OutboxEntity
            {
                Id = Guid.NewGuid(),
                OccurredOn = DateTime.UtcNow,
                EventType = domainEvent.GetType().FullName!,
                Data = JsonConvert.SerializeObject(domainEvent, m_SerializerSettings),
                PartitionKey = domainEvent.PartitionKey
            })
            .ToList();

        dbContext.Set<OutboxEntity>().AddRange(outboxMessages);

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}
