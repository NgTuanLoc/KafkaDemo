using Confluent.Kafka;

var builder = WebApplication.CreateBuilder(args);

builder.AddKafkaProducer<string, string>("messaging");
// Add services to the container.

var app = builder.Build();

// Configure the HTTP request pipeline.

app.MapGet("/", async (IProducer<string, string> producer) =>
{
    var message = new Message<string, string>()
    {
        Key = Guid.NewGuid().ToString(),
        Value = Guid.NewGuid().ToString()
    };
    var topicPartition = new TopicPartition("my-topic", new Partition(3));
    await producer.ProduceAsync(topicPartition, message);
    return "Producer";
});

await app.RunAsync();
