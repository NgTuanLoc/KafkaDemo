using Microsoft.Extensions.Configuration;

var builder = DistributedApplication.CreateBuilder(args);

var messaging = builder.AddKafka("messaging", port: 9092)
                       .WithKafkaUI(kafkaUI => kafkaUI.WithHostPort(9100))
                       .WithLifetime(ContainerLifetime.Persistent);

var username = builder.AddParameter("username", "postgres");
var password = builder.AddParameter("password", builder.Configuration.GetValue<string>("PostgresPassword")!);

var citusServer = builder
    .AddPostgres(name: "postgresql", username, password: password)
    .WithAnnotation(new ContainerImageAnnotation { Image = "citusdata/citus", Tag = "12.1" })
    .WithVolume("VolumeMount.postgres.data", "/var/lib/postgresql/data")
    .WithEndpoint("tcp", (e) =>
    {
        e.Port = 15432;
        e.IsProxied = false;
    })
    .WithLifetime(ContainerLifetime.Persistent)
    .WithPgAdmin();

var postgres = citusServer.AddDatabase("postgres");

builder.AddProject<Projects.Producer>("producer")
    .WithReference(messaging).WaitFor(messaging)
    .WithReference(postgres).WaitFor(postgres);

builder.AddProject<Projects.Consumer>("consumer")
    .WithReference(messaging)
    .WaitFor(messaging);

await builder.Build().RunAsync();
