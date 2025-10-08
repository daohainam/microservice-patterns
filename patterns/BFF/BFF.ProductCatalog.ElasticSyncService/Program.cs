using BFF.ProductCatalog.ElasticSyncService;
using BFF.ProductCatalog.Events;
using BFF.ProductCatalog.Search;
using EventBus;
using EventBus.Kafka;
using Mediator;
using MicroservicePatterns.Shared;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddMediator(cfg => {
    cfg.ServiceAssemblies.Add(typeof(ProductCreatedEvent).Assembly);
});

var eventConsumingTopics = builder.Configuration.GetValue<string>(Consts.Env_EventConsumingTopics);
if (!string.IsNullOrEmpty(eventConsumingTopics))
{
    builder.AddKafkaEventConsumer(options => {
        options.ServiceName = "ProductCatalog_SyncService";
        options.KafkaGroupId = "bff";
        options.Topics.AddRange(eventConsumingTopics.Split(','));
        options.IntegrationEventFactory = IntegrationEventFactory<ProductCreatedEvent>.Instance;
    });
}

builder.AddElasticsearchClient(connectionName: "elastic",
    configureClientSettings: (settings) =>
    {
        settings.DefaultMappingFor<ProductIndexDocument>(m => m.IndexName(nameof(ProductIndexDocument).ToLower()));
    }
);

builder.Services.AddHostedService<MessageConsumingServiceWorker>();

var host = builder.Build();
host.Run();
