var builder = DistributedApplication.CreateBuilder(args);

var messaging = builder.AddKafka("messaging")
                       .WithKafkaUI();

builder.AddProject<Projects.Producer>("producer")
    .WithReference(messaging);

builder.AddProject<Projects.Consumer>("consumer")
    .WithReference(messaging);

await builder.Build().RunAsync();
