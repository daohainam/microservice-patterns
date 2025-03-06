namespace MicroservicePatterns.AppHost.Extensions;
public static class ExternalServiceRegistrationExtentions
{
    private const string DefaultDatabase = "DefaultDatabase";
    private const string EnvKafkaTopic = "KAFKA_TOPIC";

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
            .WithEnvironment(EnvKafkaTopic, GetTopicName<Projects.CQRS_Library_BorrowerService>())
            .WithReference(kafka)
            .WithReference(borrowerDb, DefaultDatabase)
            .WaitFor(borrowerDb)
            .WaitFor(kafka);

        var bookDb = postgres.AddDefaultDatabase<Projects.CQRS_Library_BookService>();
        builder.AddProject<Projects.CQRS_Library_BookService>()
            .WithEnvironment(EnvKafkaTopic, GetTopicName<Projects.CQRS_Library_BookService>())
            .WithReference(kafka)
            .WithReference(bookDb, DefaultDatabase)
            .WaitFor(bookDb)
            .WaitFor(kafka);

        var borrowingDb = postgres.AddDefaultDatabase<Projects.CQRS_Library_BorrowingService>();
        builder.AddProject<Projects.CQRS_Library_BorrowingService>()
            .WithEnvironment(EnvKafkaTopic, GetTopicName<Projects.CQRS_Library_BorrowingService>())
            .WithReference(kafka)
            .WithReference(borrowingDb, DefaultDatabase)
            .WaitFor(borrowingDb)
            .WaitFor(kafka);

        var borrowingHistoryDb = postgres.AddDefaultDatabase<Projects.CQRS_Library_BorrowingHistoryService>();
        builder.AddProject<Projects.CQRS_Library_BorrowingHistoryService>()
            .WithEnvironment("KAFKA_INCOMMING_TOPICS", 
                string.Join(',', 
                    GetTopicName<Projects.CQRS_Library_BorrowerService>(),
                    GetTopicName<Projects.CQRS_Library_BookService>(),
                    GetTopicName<Projects.CQRS_Library_BorrowingService>()
                    )
                )
            .WithReference(kafka)
            .WithReference(borrowingHistoryDb, DefaultDatabase)
            .WaitFor(borrowingHistoryDb)
            .WaitFor(kafka);

        var sagaCatalogDb = postgres.AddDefaultDatabase<Projects.Saga_OnlineStore_CatalogService>();
        builder.AddProject<Projects.Saga_OnlineStore_CatalogService>()
            .WithEnvironment(EnvKafkaTopic, GetTopicName<Projects.Saga_OnlineStore_CatalogService>())
            .WithReference(kafka)
            .WithReference(sagaCatalogDb, DefaultDatabase)
            .WaitFor(sagaCatalogDb)
            .WaitFor(kafka);

        var sagaInventoryDb = postgres.AddDefaultDatabase<Projects.Saga_OnlineStore_InventoryService>();
        builder.AddProject<Projects.Saga_OnlineStore_InventoryService>()
            .WithEnvironment(EnvKafkaTopic, GetTopicName<Projects.Saga_OnlineStore_InventoryService>())
            .WithEnvironment("KAFKA_CATALOG_TOPIC", GetTopicName<Projects.Saga_OnlineStore_CatalogService>())
            .WithReference(kafka)
            .WithReference(sagaInventoryDb, DefaultDatabase)
            .WaitFor(sagaInventoryDb)
            .WaitFor(kafka);

        var sagaBankCardDb = postgres.AddDefaultDatabase<Projects.Saga_OnlineStore_BankCardService>();
        builder.AddProject<Projects.Saga_OnlineStore_BankCardService>()
            .WithEnvironment(EnvKafkaTopic, GetTopicName<Projects.Saga_OnlineStore_BankCardService>())
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
            new() { Name = GetTopicName<Projects.CQRS_Library_BookService>(), NumPartitions = 1, ReplicationFactor = 1 },
            new() { Name = GetTopicName<Projects.CQRS_Library_BorrowerService>(), NumPartitions = 1, ReplicationFactor = 1 },
            new() { Name = GetTopicName<Projects.CQRS_Library_BorrowingService>(), NumPartitions = 1, ReplicationFactor = 1 },
            new() { Name = GetTopicName<Projects.Saga_OnlineStore_CatalogService>(), NumPartitions = 1, ReplicationFactor = 1 },
            new() { Name = GetTopicName<Projects.Saga_OnlineStore_BankCardService>(), NumPartitions = 1, ReplicationFactor = 1 },
            new() { Name = GetTopicName<Projects.Saga_OnlineStore_InventoryService>(), NumPartitions = 1, ReplicationFactor = 1 }
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

    private static string GetTopicName<TProject>() => typeof(TProject).Name.Replace('_', '-');
}
