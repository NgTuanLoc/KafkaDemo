using MassTransit;
using SharedLibrary;
using Microsoft.EntityFrameworkCore;
using Producer;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<ConvertDomainEventsToOutboxMessagesInterceptor>();

builder.Services.AddDbContext<ProducerDbContext>((options, optionsBuilder) =>
{
    var connString = builder.Configuration.GetConnectionString("postgres");
    var interceptor = options.GetRequiredService<ConvertDomainEventsToOutboxMessagesInterceptor>();

    optionsBuilder.UseNpgsql(connString, sqlOptions =>
    {
        sqlOptions.MigrationsHistoryTable("__MigrationsHistory", "products");
        sqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
    }).AddInterceptors(interceptor);
});

builder.Services.AddCronJob<ProcessOutboxMessagesJob>(c =>
{
    c.TimeZoneInfo = TimeZoneInfo.Utc;
    c.CronExpression = "* * * * *";
});

AddEventBus(builder);

var app = builder.Build();

//// Configure the HTTP request pipeline
//app.MapGet("/", async (ITopicProducer<MyMessage> producer) =>
//{
//    var message = new MyMessage()
//    {
//        Text = Guid.NewGuid().ToString()
//    };

//    await producer.Produce(message); // Publish message using MassTransit
//    return "Producer";
//});

// Configure the HTTP request pipeline
app.MapGet("/", async (ProducerDbContext context, CancellationToken cancellationToken) =>
{
    var id = Guid.NewGuid();
    var product = new ProductEntity()
    {
        Name = $"Name {id}",
        Description = $"Description {id}"
    };
    product.RegisterDomainEvent(new ProductCreated(product.Id, product.Name, product.Description));
    context.Products.Add(product);
    await context.SaveChangesAsync(cancellationToken);
    return Results.Created();
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
            // Register the Event Hub producer
            rider.AddProducer<ProductEntity>("my-event-hub");

            rider.UsingEventHub((context, cfg) =>
            {
                cfg.Host(builder.Configuration.GetConnectionString("event-hub"));

            });
        });
    });
}
