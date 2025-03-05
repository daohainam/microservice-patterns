using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MicroservicePatterns.AppHost.Extensions;
public static class ExternalServiceRegistrationExtentions
{
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

        var borrowerDb = postgres.AddDatabase("cqrs-library-borrower-db");
        var borrowerApi = builder.AddProject<Projects.CQRS_Library_BorrowerService>("cqrs-library-borrower-service")
            .WithReference(kafka)
            .WithReference(borrowerDb)
            .WaitFor(borrowerDb)
            .WaitFor(kafka);

        var bookDb = postgres.AddDatabase("cqrs-library-book-db");
        builder.AddProject<Projects.CQRS_Library_BookService>("cqrs-library-book-service")
            .WithReference(kafka)
            .WithReference(bookDb)
            .WaitFor(bookDb)
            .WaitFor(kafka);

        var borrowingDb = postgres.AddDatabase("cqrs-library-borrowing-db");
        builder.AddProject<Projects.CQRS_Library_BorrowingService>("cqrs-library-borrowing-service")
            .WithReference(kafka)
            .WithReference(borrowingDb)
            .WaitFor(borrowingDb)
            .WaitFor(kafka);

        var borrowingHistoryDb = postgres.AddDatabase("cqrs-library-borrowing-history-db");
        builder.AddProject<Projects.CQRS_Library_BorrowingHistoryService>("cqrs-library-borrowing-history-service")
            .WithReference(kafka)
            .WithReference(borrowingHistoryDb)
            .WaitFor(borrowingHistoryDb)
            .WaitFor(kafka);

        var sagaCatalogDb = postgres.AddDatabase("saga-onlinestore-catalog-db");
        builder.AddProject<Projects.Saga_OnlineStore_CatalogService>("saga-onlinestore-catalog-service")
            .WithReference(kafka)
            .WithReference(sagaCatalogDb)
            .WaitFor(sagaCatalogDb)
            .WaitFor(kafka);

        var sagaInventoryDb = postgres.AddDatabase("saga-onlinestore-inventory-db");
        builder.AddProject<Projects.Saga_OnlineStore_InventoryService>("saga-onlinestore-inventory-service")
            .WithReference(kafka)
            .WithReference(sagaInventoryDb)
            .WaitFor(sagaInventoryDb)
            .WaitFor(kafka);

        var sagaBankCardDb = postgres.AddDatabase("saga-onlinestore-bankcard-db");
        builder.AddProject<Projects.Saga_OnlineStore_BankCardService>("saga-onlinestore-bankcard-service")
            .WithReference(kafka)
            .WithReference(sagaBankCardDb)
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
