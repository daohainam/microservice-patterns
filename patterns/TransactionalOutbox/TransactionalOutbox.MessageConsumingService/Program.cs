using MicroservicePatterns.Shared;
using TransactionalOutbox.MessageConsumingService;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddSingleton(new MessageConsumingServiceOptions() {
    BootstrapServers = builder.Configuration.GetConnectionString("kafka") ?? throw new Exception("kafka connection string required"), 
    GroupId = "TransactionalOutbox",
    Topic = builder.Configuration.GetValue<string>(Consts.Env_EventConsumingTopics) ?? throw new Exception("Kafka topic required")
});

builder.Services.AddHostedService<MessageConsumingServiceWorker>();

var host = builder.Build();
host.Run();
