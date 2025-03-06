namespace MicroservicePatterns.AppHost.Extensions;
public static class ExternalServiceRegistrationExtentions
{
    private const string DefaultDatabase = "DefaultDatabase";

    public static IDistributedApplicationBuilder AddApplicationServices(this IDistributedApplicationBuilder builder)
    {
        var cache = builder.AddRedis("redis").WithLifetime(ContainerLifetime.Persistent).WithRedisInsight();
        var kafka = builder.AddKafka("kafka").WithLifetime(ContainerLifetime.Persistent).WithKafkaUI();
        var mongoDb = builder.AddMongoDB("mongodb").WithLifetime(ContainerLifetime.Persistent).WithMongoExpress().WithDataVolume(); // here we use MongoDB for both read/write model, but we can use different databases using replicas
        var postgres = builder.AddPostgres("postgresql").WithLifetime(ContainerLifetime.Persistent).WithPgWeb().WithDataVolume();

        builder.Eventing.Subscribe<ResourceReadyEvent>(kafka.Resource, async (@event, ct) =>
        {
            await CreateKafkaTopics(@event, kafka.Resource, ct);
        });

        var borrowerDb = postgres.AddDefaultDatabase<Projects.CQRS_Library_BorrowerService>();
        var borrowerApi = builder.AddProject<Projects.CQRS_Library_BorrowerService>()
            .WithReference(kafka)
            .WithReference(borrowerDb, DefaultDatabase)
            .WaitFor(borrowerDb)
            .WaitFor(kafka);

        var bookDb = postgres.AddDefaultDatabase<Projects.CQRS_Library_BookService>();
        builder.AddProject<Projects.CQRS_Library_BookService>()
            .WithReference(kafka)
            .WithReference(bookDb, DefaultDatabase)
            .WaitFor(bookDb)
            .WaitFor(kafka);

        var borrowingDb = postgres.AddDefaultDatabase<Projects.CQRS_Library_BorrowingService>();
        builder.AddProject<Projects.CQRS_Library_BorrowingService>()
            .WithReference(kafka)
            .WithReference(borrowingDb, DefaultDatabase)
            .WaitFor(borrowingDb)
            .WaitFor(kafka);

        var borrowingHistoryDb = postgres.AddDefaultDatabase<Projects.CQRS_Library_BorrowingHistoryService>();
        builder.AddProject<Projects.CQRS_Library_BorrowingHistoryService>()
            .WithReference(kafka)
            .WithReference(borrowingHistoryDb, DefaultDatabase)
            .WaitFor(borrowingHistoryDb)
            .WaitFor(kafka);

        var sagaCatalogDb = postgres.AddDefaultDatabase<Projects.Saga_OnlineStore_CatalogService>();
        builder.AddProject<Projects.Saga_OnlineStore_CatalogService>()
            .WithReference(kafka)
            .WithReference(sagaCatalogDb, DefaultDatabase)
            .WaitFor(sagaCatalogDb)
            .WaitFor(kafka);

        var sagaInventoryDb = postgres.AddDefaultDatabase<Projects.Saga_OnlineStore_InventoryService>();
        builder.AddProject<Projects.Saga_OnlineStore_InventoryService>()
            .WithReference(kafka)
            .WithReference(sagaInventoryDb, DefaultDatabase)
            .WaitFor(sagaInventoryDb)
            .WaitFor(kafka);

        var sagaBankCardDb = postgres.AddDefaultDatabase<Projects.Saga_OnlineStore_BankCardService>();
        builder.AddProject<Projects.Saga_OnlineStore_BankCardService>()
            .WithReference(kafka)
            .WithReference(sagaBankCardDb, DefaultDatabase)
            .WaitFor(sagaBankCardDb)
            .WaitFor(kafka);

        return builder;
    }

    private static async Task CreateKafkaTopics(ResourceReadyEvent @event, KafkaServerResource kafkaResource, CancellationToken ct)
    {
        var logger = @event.Services.GetRequiredService<ILogger<Program>>();

        TopicSpecification[] topics = [
            new() { Name = "cqrs-library-book", NumPartitions = 1, ReplicationFactor = 1 },
                    new() { Name = "cqrs-library-borrower", NumPartitions = 1, ReplicationFactor = 1 },
                    new() { Name = "cqrs-library-borrowing", NumPartitions = 1, ReplicationFactor = 1 },
                    new() { Name = "saga-onlinestore-catalog", NumPartitions = 1, ReplicationFactor = 1 },
                    new() { Name = "saga-onlinestore-inventory", NumPartitions = 1, ReplicationFactor = 1 },
                    new() { Name = "saga-onlinestore-bankcard", NumPartitions = 1, ReplicationFactor = 1 }
        ];

        logger.LogInformation("Creating topics: {topics} ...", string.Join(", ", topics.Select(t => t.Name).ToArray()));

        var connectionString = await kafkaResource.ConnectionStringExpression.GetValueAsync(ct);
        using var adminClient = new AdminClientBuilder(new AdminClientConfig()
        {
            BootstrapServers = connectionString,
        }).Build();
        try
        {
            await adminClient.CreateTopicsAsync(topics);
        }
        catch (CreateTopicsException)
        {
            logger.LogError("An error occurred creating topics");

            throw;
        }
    }
}
