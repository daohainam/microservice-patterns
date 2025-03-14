using MicroservicePatterns.Shared;
using Microsoft.Extensions.Configuration;

namespace MicroservicePatterns.AppHost.Extensions;
public static class ExternalServiceRegistrationExtentions
{
    private const string Choreography = "Choreography";
    private const string Orchestration = "Orchestration";

    public static IDistributedApplicationBuilder AddApplicationServices(this IDistributedApplicationBuilder builder)
    {
        var cache = builder.AddRedis("redis");
        var kafka = builder.AddKafka("kafka");
        var mongoDb = builder.AddMongoDB("mongodb"); 
        var postgres = builder.AddPostgres("postgresql");

        if (!builder.Configuration.GetValue("IsTest", false))
        {
            cache = cache.WithLifetime(ContainerLifetime.Persistent).WithDataVolume().WithRedisInsight();
            kafka = kafka.WithLifetime(ContainerLifetime.Persistent).WithDataVolume().WithKafkaUI();
            mongoDb = mongoDb.WithLifetime(ContainerLifetime.Persistent).WithDataVolume().WithMongoExpress();
            postgres = postgres.WithLifetime(ContainerLifetime.Persistent).WithDataVolume().WithPgWeb();
        }

        builder.Eventing.Subscribe<ResourceReadyEvent>(kafka.Resource, async (@event, ct) =>
        {
            await CreateKafkaTopics(@event, kafka.Resource, ct);
        });

        #region CQRS Library
        var borrowerDb = postgres.AddDefaultDatabase<Projects.CQRS_Library_BorrowerService>();
        var borrowerApi = builder.AddProjectWithPostfix<Projects.CQRS_Library_BorrowerService>()
            .WithEnvironment(Consts.Env_EventPublishingTopics, GetTopicName<Projects.CQRS_Library_BorrowerService>())
            .WithReference(kafka)
            .WithReference(borrowerDb, Consts.DefaultDatabase)
            .WaitFor(borrowerDb)
            .WaitFor(kafka);

        var bookDb = postgres.AddDefaultDatabase<Projects.CQRS_Library_BookService>();
        builder.AddProjectWithPostfix<Projects.CQRS_Library_BookService>()
            .WithEnvironment(Consts.Env_EventPublishingTopics, GetTopicName<Projects.CQRS_Library_BookService>())
            .WithReference(kafka)
            .WithReference(bookDb, Consts.DefaultDatabase)
            .WaitFor(bookDb)
            .WaitFor(kafka);

        var borrowingDb = postgres.AddDefaultDatabase<Projects.CQRS_Library_BorrowingService>();
        builder.AddProjectWithPostfix<Projects.CQRS_Library_BorrowingService>()
            .WithEnvironment(Consts.Env_EventPublishingTopics, GetTopicName<Projects.CQRS_Library_BorrowingService>())
            .WithReference(kafka)
            .WithReference(borrowingDb, Consts.DefaultDatabase)
            .WaitFor(borrowingDb)
            .WaitFor(kafka);

        var borrowingHistoryDb = postgres.AddDefaultDatabase<Projects.CQRS_Library_BorrowingHistoryService>();
        builder.AddProjectWithPostfix<Projects.CQRS_Library_BorrowingHistoryService>()
            .WithEnvironment(Consts.Env_EventConsumingTopics, 
                string.Join(',', 
                    GetTopicName<Projects.CQRS_Library_BorrowerService>(),
                    GetTopicName<Projects.CQRS_Library_BookService>(),
                    GetTopicName<Projects.CQRS_Library_BorrowingService>()
                    )
                )
            .WithReference(kafka)
            .WithReference(borrowingHistoryDb, Consts.DefaultDatabase)
            .WaitFor(borrowingHistoryDb)
            .WaitFor(kafka);
        #endregion CQRS Library

        #region Saga Online Store - Choreography
        var sagaCatalogDb = postgres.AddDefaultDatabase<Projects.Saga_OnlineStore_CatalogService>();
        builder.AddProjectWithPostfix<Projects.Saga_OnlineStore_CatalogService>(Choreography)
            .WithEnvironment(Consts.Env_EventPublishingTopics, GetTopicName<Projects.Saga_OnlineStore_CatalogService>(Choreography))
            .WithReference(kafka)
            .WithReference(sagaCatalogDb, Consts.DefaultDatabase)
            .WaitFor(sagaCatalogDb)
            .WaitFor(kafka);

        var sagaInventoryDb = postgres.AddDefaultDatabase<Projects.Saga_OnlineStore_InventoryService>();
        builder.AddProjectWithPostfix<Projects.Saga_OnlineStore_InventoryService>(Choreography)
            .WithEnvironment(Consts.Env_EventPublishingTopics, GetTopicName<Projects.Saga_OnlineStore_InventoryService>(Choreography))
            .WithEnvironment(Consts.Env_EventConsumingTopics,
                string.Join(',',
                    GetTopicName<Projects.Saga_OnlineStore_CatalogService>(Choreography),
                    GetTopicName<Projects.Saga_OnlineStore_OrderService>(Choreography),
                    GetTopicName<Projects.Saga_OnlineStore_PaymentService>(Choreography)
                    )
                )
            .WithReference(kafka)
            .WithReference(sagaInventoryDb, Consts.DefaultDatabase)
            .WaitFor(sagaInventoryDb)
            .WaitFor(kafka);

        var sagaBankCardDb = postgres.AddDefaultDatabase<Projects.Saga_OnlineStore_PaymentService>();
        builder.AddProjectWithPostfix<Projects.Saga_OnlineStore_PaymentService>(Choreography)
            .WithEnvironment(Consts.Env_EventPublishingTopics, GetTopicName<Projects.Saga_OnlineStore_PaymentService>(Choreography))
            .WithEnvironment(Consts.Env_EventConsumingTopics,
                string.Join(',',
                    GetTopicName<Projects.Saga_OnlineStore_InventoryService>(Choreography)
                    )
                )
            .WithReference(kafka)
            .WithReference(sagaBankCardDb, Consts.DefaultDatabase)
            .WaitFor(sagaBankCardDb)
            .WaitFor(kafka);

        var sagaOrderDb = postgres.AddDefaultDatabase<Projects.Saga_OnlineStore_OrderService>();
        builder.AddProjectWithPostfix<Projects.Saga_OnlineStore_OrderService>(Choreography)
            .WithEnvironment(Consts.Env_EventPublishingTopics, GetTopicName<Projects.Saga_OnlineStore_OrderService>(Choreography))
            .WithEnvironment(Consts.Env_EventConsumingTopics,
                string.Join(',',
                    GetTopicName<Projects.Saga_OnlineStore_InventoryService>(Choreography),
                    GetTopicName<Projects.Saga_OnlineStore_PaymentService>(Choreography)
                    )
                )
            .WithReference(kafka)
            .WithReference(sagaOrderDb, Consts.DefaultDatabase)
            .WaitFor(sagaOrderDb)
            .WaitFor(kafka);

        #endregion

        #region Saga Online Store - Orchestration
        builder.AddProjectWithPostfix<Projects.Saga_OnlineStore_CatalogService>(Orchestration)
            .WithEnvironment(Consts.Env_EventPublishingTopics, GetTopicName<Projects.Saga_OnlineStore_CatalogService>(Orchestration))
            .WithReference(kafka)
            .WithReference(sagaCatalogDb, Consts.DefaultDatabase)
            .WaitFor(sagaCatalogDb)
            .WaitFor(kafka);

        builder.AddProjectWithPostfix<Projects.Saga_OnlineStore_InventoryService>(Orchestration)
            .WithEnvironment(Consts.Env_EventPublishingTopics, GetTopicName<Projects.Saga_OnlineStore_InventoryService>(Orchestration))
            .WithEnvironment(Consts.Env_EventConsumingTopics,
                string.Join(',',
                    GetTopicName<Projects.Saga_OnlineStore_CatalogService>(Orchestration),
                    GetTopicName<Projects.Saga_OnlineStore_OrderService>(Orchestration),
                    GetTopicName<Projects.Saga_OnlineStore_PaymentService>( Orchestration)
                    )
                )
            .WithReference(kafka)
            .WithReference(sagaInventoryDb, Consts.DefaultDatabase)
            .WaitFor(sagaInventoryDb)
            .WaitFor(kafka);

        builder.AddProjectWithPostfix<Projects.Saga_OnlineStore_PaymentService>(Orchestration)
            .WithEnvironment(Consts.Env_EventPublishingTopics, GetTopicName<Projects.Saga_OnlineStore_PaymentService>(Orchestration))
            .WithEnvironment(Consts.Env_EventConsumingTopics,
                string.Join(',',
                    GetTopicName<Projects.Saga_OnlineStore_InventoryService>(Orchestration)
                    )
                )
            .WithReference(kafka)
            .WithReference(sagaBankCardDb, Consts.DefaultDatabase)
            .WaitFor(sagaBankCardDb)
            .WaitFor(kafka);

        builder.AddProjectWithPostfix<Projects.Saga_OnlineStore_OrderService>(Orchestration)
            .WithEnvironment(Consts.Env_EventPublishingTopics, GetTopicName<Projects.Saga_OnlineStore_OrderService>(Orchestration))
            .WithEnvironment(Consts.Env_EventConsumingTopics,
                string.Join(',',
                    GetTopicName<Projects.Saga_OnlineStore_InventoryService>(Orchestration),
                    GetTopicName<Projects.Saga_OnlineStore_PaymentService>(Orchestration)
                    )
                )
            .WithReference(kafka)
            .WithReference(sagaOrderDb, Consts.DefaultDatabase)
            .WaitFor(sagaOrderDb)
            .WaitFor(kafka);

        #endregion

        return builder;
    }

    private static async Task CreateKafkaTopics(ResourceReadyEvent @event, KafkaServerResource kafkaResource, CancellationToken ct)
    {
        var logger = @event.Services.GetRequiredService<ILogger<Program>>();

        TopicSpecification[] topics = [
            new() { Name = GetTopicName<Projects.CQRS_Library_BookService>(), NumPartitions = 1, ReplicationFactor = 1 },
            new() { Name = GetTopicName<Projects.CQRS_Library_BorrowerService>(), NumPartitions = 1, ReplicationFactor = 1 },
            new() { Name = GetTopicName<Projects.CQRS_Library_BorrowingService>(), NumPartitions = 1, ReplicationFactor = 1 },
            new() { Name = GetTopicName<Projects.Saga_OnlineStore_CatalogService>(Choreography), NumPartitions = 1, ReplicationFactor = 1 },
            new() { Name = GetTopicName<Projects.Saga_OnlineStore_PaymentService>(Choreography), NumPartitions = 1, ReplicationFactor = 1 },
            new() { Name = GetTopicName<Projects.Saga_OnlineStore_InventoryService>(Choreography), NumPartitions = 1, ReplicationFactor = 1 },
            new() { Name = GetTopicName<Projects.Saga_OnlineStore_OrderService>(Choreography), NumPartitions = 1, ReplicationFactor = 1 }
        ];

        logger.LogInformation("Creating topics: {topics} ...", string.Join(", ", topics.Select(t => t.Name).ToArray()));

        var connectionString = await kafkaResource.ConnectionStringExpression.GetValueAsync(ct);
        using var adminClient = new AdminClientBuilder(new AdminClientConfig()
        {
            BootstrapServers = connectionString,
        }).Build();
        try
        {
            await adminClient.CreateTopicsAsync(topics, new CreateTopicsOptions() { });
        }
        catch (CreateTopicsException ex)
        {
            logger.LogError(ex, "An error occurred creating topics");
        }
    }

    private static string GetTopicName<TProject>(string postfix = "") => $"{typeof(TProject).Name.Replace('_', '-')}{(string.IsNullOrEmpty(postfix) ? "" : $"-{postfix}")}";
}
