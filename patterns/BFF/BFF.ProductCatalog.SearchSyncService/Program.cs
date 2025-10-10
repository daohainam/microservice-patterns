using BFF.ProductCatalog.SearchSyncService;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddMediator(cfg =>
{
    cfg.ServiceAssemblies.Add(typeof(ProductCreatedEvent).Assembly);
});

var eventConsumingTopics = builder.Configuration.GetValue<string>(Consts.Env_EventConsumingTopics);
if (!string.IsNullOrEmpty(eventConsumingTopics))
{
    builder.AddKafkaEventConsumer(options =>
    {
        options.ServiceName = "ProductCatalogSyncService";
        options.KafkaGroupId = "bff-search";
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

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();

