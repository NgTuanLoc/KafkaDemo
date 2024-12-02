using Confluent.Kafka;
using Consumer;
using MassTransit;
using SharedLibrary;

var builder = WebApplication.CreateBuilder(args);
builder.AddKafkaConsumer<string, string>("messaging");

AddEventBus(builder);
//builder.Services.AddHostedService<EventConsumerJob>();
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

static void AddEventBus(WebApplicationBuilder builder)
{
    // Configure MassTransit with Kafka Rider
    builder.Services.AddMassTransit(config =>
    {
        config.UsingInMemory((context, cfg) =>
        {
            cfg.ConfigureEndpoints(context);
        });

        config.AddRider(rider =>
        {
            rider.AddConsumer<MessageConsumer>();

            rider.UsingKafka((context, k) =>
            {
                k.Host(builder.Configuration.GetConnectionString("kafka-producer")); // Kafka broker address
                k.TopicEndpoint<ProductEntity>("my-topic", "consumer-group-name", e =>
                {
                    e.ConfigureConsumer<MessageConsumer>(context);
                });
            });
        });
    });
}
