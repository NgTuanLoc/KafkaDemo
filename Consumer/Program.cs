using Confluent.Kafka;
using Consumer;

var builder = WebApplication.CreateBuilder(args);
builder.AddKafkaConsumer<string, string>("messaging");
builder.Services.AddHostedService<EventConsumerJob>();
// Add services to the container.

var app = builder.Build();

// Configure the HTTP request pipeline.

app.MapGet("/", (IConsumer<string, string> consumer) =>
{
    consumer.Subscribe(["my-topic"]);
    var test = consumer.Consume(TimeSpan.FromMinutes(5000));
    Console.WriteLine($"CONSUME {test.Partition.Value}");
    Console.WriteLine(test.Message.Value);

    return "Consumer";
});

await app.RunAsync();
