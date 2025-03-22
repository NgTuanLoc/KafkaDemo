
using Dapper;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using SharedLibrary;
using SharedLibrary.TransactionalOutbox;
using System.Data;
using System.Text.Json;

namespace Producer;

public class ProcessOutboxMessagesJob(IScheduleConfig<ProcessOutboxMessagesJob> jobConfig, IConfiguration config, ILogger<ProcessOutboxMessagesJob> logger, IServiceScopeFactory scopeFactory)
    : CronJobService(jobConfig.CronExpression, jobConfig.TimeZoneInfo, logger)
{
    private readonly NpgsqlDataSource dataSource = NpgsqlDataSource.Create(config.GetConnectionString("postgres") ?? throw new ArgumentNullException(nameof(config), "Connection string is required!"));
    private readonly int BatchSize = 10;
    public override Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("CronJob 1 starts.");
        return base.StartAsync(cancellationToken);
    }

    public override async Task DoWork(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var producer = scope.ServiceProvider.GetRequiredService<ITopicProducer<ProductEntity>>();

        string query = "SELECT * FROM \"EventStreaming\".transactional_outbox ORDER BY \"OccurredOn\" LIMIT @BatchSize FOR UPDATE SKIP LOCKED";

        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            var messages = await connection.QueryAsync<OutboxEntity>(query, new { BatchSize }, transaction: transaction);

            if (!messages.Any()) return;

            foreach (var message in messages)
            {
                var domainEvent = JsonSerializer.Deserialize<ProductEntity>(message.Data)!;
                await producer.Produce(domainEvent, cancellationToken);
                await connection.ExecuteAsync("DELETE FROM \"EventStreaming\".transactional_outbox WHERE \"Id\" = @Id", new { message.Id }, transaction: transaction);
            }

            await transaction.CommitAsync(cancellationToken);

            logger.LogInformation("{Now} CronJob is working.", DateTime.Now.ToString("T"));
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            logger.LogError(ex, "Error occurred while processing the outbox messages.");
            throw;
        }
    }


    public override Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("CronJob 1 is stopping.");
        return base.StopAsync(cancellationToken);
    }
}