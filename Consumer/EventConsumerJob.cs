using Confluent.Kafka;

namespace Consumer;

public class EventConsumerJob(ILogger<EventConsumerJob> logger, IConsumer<string, string> consumer) : BackgroundService
{
    private readonly ILogger<EventConsumerJob> _logger = logger;
    private readonly IConsumer<string, string> _consumer = consumer;

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _consumer.Subscribe("my-topic");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var consumeResult = _consumer.Consume(TimeSpan.FromSeconds(5));

                if (consumeResult == null)
                {
                    continue;
                }

                _logger.LogInformation("Consumed message '{MessageValue}' at: '{Offset}'", consumeResult.Message.Value, consumeResult.Offset);
            }
            catch (Exception)
            {
                // Ignore
            }
        }
        return Task.CompletedTask;
    }
}

//kafka-topics --create --topic loc-topic --partitions 4 --replication-factor 1 --bootstrap-server localhost:9093