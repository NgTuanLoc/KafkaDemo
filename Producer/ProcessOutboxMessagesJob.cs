
using MassTransit;
using MassTransit.KafkaIntegration;
using Microsoft.EntityFrameworkCore;
using SharedLibrary;
using SharedLibrary.TransactionalOutbox;
using System.Text.Json;

namespace Producer;

public class ProcessOutboxMessagesJob(IScheduleConfig<ProcessOutboxMessagesJob> config, ILogger<ProcessOutboxMessagesJob> logger, IServiceScopeFactory scopeFactory)
    : CronJobService(config.CronExpression, config.TimeZoneInfo, logger)
{
    public override Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("CronJob 1 starts.");
        return base.StartAsync(cancellationToken);
    }

    public override async Task DoWork(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ProducerDbContext>();

        var items = await dbContext.Set<OutboxEntity>()
            .OrderBy(o => o.OccurredOn)
            .Take(2)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        List<ProductEntity> domainEvents = [];

        if (items.Count == 0) return;

        foreach (var item in items)
        {
            domainEvents.Add(JsonSerializer.Deserialize<ProductEntity>(item.Data)!);
        }

        var producer = scope.ServiceProvider.GetRequiredService<ITopicProducer<ProductEntity>>();

        await producer.Produce(domainEvents, Pipe.Execute<KafkaSendContext>(x =>
        {

            var sendContext = (x as KafkaMessageSendContext<string, ProductEntity>);
            sendContext.Key = "AAAAA";
        }), cancellationToken);

        dbContext.Set<OutboxEntity>().RemoveRange(items);
        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation("{Now} CronJob is working.", DateTime.Now.ToString("T"));
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("CronJob 1 is stopping.");
        return base.StopAsync(cancellationToken);
    }
}