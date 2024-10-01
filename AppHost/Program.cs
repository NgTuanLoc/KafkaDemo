var builder = DistributedApplication.CreateBuilder(args);

var messaging = builder.AddKafka("messaging")
                       .WithKafkaUI();

await builder.Build().RunAsync();
