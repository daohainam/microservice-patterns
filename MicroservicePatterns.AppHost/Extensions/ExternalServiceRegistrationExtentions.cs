using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aspire.Hosting;
using Confluent.Kafka.Admin;
using Confluent.Kafka;
using MongoDB.Driver;
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
            var logger = @event.Services.GetRequiredService<ILogger<Program>>();
            logger.LogInformation("Creating topics...");

            var connectionString = await kafka.Resource.ConnectionStringExpression.GetValueAsync(ct);
            using var adminClient = new AdminClientBuilder(new AdminClientConfig() {
                BootstrapServers = connectionString,
            }).Build();
            try
            {
                await adminClient.CreateTopicsAsync(
                [
                new() { Name = "book", NumPartitions = 1, ReplicationFactor = 1 },
                new() { Name = "borrower", NumPartitions = 1, ReplicationFactor = 1 },
                new() { Name = "borrowing", NumPartitions = 1, ReplicationFactor = 1 }
                ]);
            }
            catch (CreateTopicsException)
            {
                logger.LogError("An error occurred creating topics");

                throw;
            }
        });

        var borrowerDb = postgres.AddDatabase("cqrs-borrower-db");
        var borrowerApi = builder.AddProject<Projects.CQRS_Library_BorrowerService>("cqrs-library-borrower-service")
            .WithReference(kafka)
            .WithReference(borrowerDb)
            .WaitFor(borrowerDb)
            .WaitFor(kafka);

        var bookDb = postgres.AddDatabase("cqrs-book-db");
        builder.AddProject<Projects.CQRS_Library_BookService>("cqrs-library-book-service")
            .WithReference(kafka)
            .WithReference(bookDb)
            .WaitFor(bookDb)
            .WaitFor(kafka);

        var borrowingDb = postgres.AddDatabase("cqrs-borrowing-db");
        builder.AddProject<Projects.CQRS_Library_BorrowingService>("cqrs-library-borrowing-service")
            .WithReference(kafka)
            .WithReference(borrowingDb)
            .WaitFor(borrowingDb)
            .WaitFor(kafka);

        var borrowingHistoryDb = postgres.AddDatabase("cqrs-borrowing-history-db");
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


        return builder;
    }
}
