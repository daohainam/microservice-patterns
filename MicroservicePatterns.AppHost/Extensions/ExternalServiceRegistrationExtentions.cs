using Aspire.Hosting;
using Aspire.Hosting.Yarp;
using Grpc.Core;
using MicroservicePatterns.AppHost.OpenTelemetryCollector;
using MicroservicePatterns.Shared;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;

namespace MicroservicePatterns.AppHost.Extensions;
public static class ExternalServiceRegistrationExtentions
{
    private static readonly bool GatewayDangerousAcceptAnyServerCertificate = true;

    public static IDistributedApplicationBuilder AddApplicationServices(this IDistributedApplicationBuilder builder)
    {
        var cache = builder.AddRedis("redis");
        var kafka = builder.AddKafka("kafka");
        var postgres = builder.AddPostgres("postgresql");
        //var debezium = builder.AddDebezium();

        if (!builder.Configuration.GetValue("IsTest", false))
        {
            cache = cache.WithLifetime(ContainerLifetime.Persistent).WithDataVolume().WithRedisInsight();
            kafka = kafka.WithLifetime(ContainerLifetime.Persistent).WithDataVolume().WithKafkaUI();
            postgres = postgres.WithLifetime(ContainerLifetime.Persistent).WithDataVolume().WithPgWeb();
        }

        // Grafana and Prometheus supports
        var prometheus = builder.AddContainer("prometheus", "prom/prometheus", "v3.5.0")
       .WithBindMount("../config/prometheus", "/etc/prometheus", isReadOnly: true)
       .WithArgs("--web.enable-otlp-receiver", "--config.file=/etc/prometheus/prometheus.yml")
       .WithHttpEndpoint(targetPort: 9090, name: "http");

        var grafana = builder.AddContainer("grafana", "grafana/grafana")
                             .WithBindMount("../config/grafana/config", "/etc/grafana", isReadOnly: true)
                             .WithBindMount("../config/grafana/dashboards", "/var/lib/grafana/dashboards", isReadOnly: true)
                             .WithEnvironment("PROMETHEUS_ENDPOINT", prometheus.GetEndpoint("http"))
                             .WithHttpEndpoint(targetPort: 3000, name: "http");

        builder.AddOpenTelemetryCollector("otelcollector", "../config/otelcollector/config.yaml")
               .WithEnvironment("PROMETHEUS_ENDPOINT", $"{prometheus.GetEndpoint("http")}/api/v1/otlp");



        // Uncomment the following line to create Kafka topics automatically when the Kafka server is ready.
        //builder.Eventing.Subscribe<ResourceReadyEvent>(kafka.Resource, async (@event, ct) =>
        //{
        //    await CreateKafkaTopics(@event, kafka.Resource, ct);
        //});

        #region Backend for Frontend (BFF)
        var productCategoryDb = postgres.AddDefaultDatabase<Projects.BFF_ProductCatalogService>();
        var productCategoryService = builder.AddProjectWithPostfix<Projects.BFF_ProductCatalogService>()
            // .WithReference(kafka)
            .WithReference(productCategoryDb, Consts.DefaultDatabase)
            .WaitFor(productCategoryDb)
            // .WaitFor(kafka)
            .WithEnvironment("GRAFANA_URL", grafana.GetEndpoint("http"));
        #endregion

        #region CQRS Library
        var bookDb = postgres.AddDefaultDatabase<Projects.CQRS_Library_BookService>();
        var bookService = builder.AddProjectWithPostfix<Projects.CQRS_Library_BookService>()
            .WithEnvironment(Consts.Env_EventPublishingTopics, GetTopicName<Projects.CQRS_Library_BookService>())
            .WithReference(kafka)
            .WithReference(bookDb, Consts.DefaultDatabase)
            .WaitFor(bookDb)
            .WaitFor(kafka)
            .WithEnvironment("GRAFANA_URL", grafana.GetEndpoint("http"))
            .WithHttpCommand(
            path: "/api/cqrs/v1/books",
            displayName: "Register a test book",
            commandOptions: new HttpCommandOptions()
            {
                Description = "",
                PrepareRequest = (context) =>
                {
                    context.Request.Content = JsonContent.Create(new
                    {
                        Id = "019557EE-978C-7CE8-9152-73637B6DC9F5",
                        Title = "The Fellowship of the Ring",
                        Author = "J. R. R. Tolkien",
                    });
                    return Task.CompletedTask;
                },
                GetCommandResult = GetCommandResult,
                IconName = "BookAdd",
                IsHighlighted = true
            }
        );

        var borrowerDb = postgres.AddDefaultDatabase<Projects.CQRS_Library_BorrowerService>();
        var borrowerService = builder.AddProjectWithPostfix<Projects.CQRS_Library_BorrowerService>()
            .WithEnvironment(Consts.Env_EventPublishingTopics, GetTopicName<Projects.CQRS_Library_BorrowerService>())
            .WithReference(kafka)
            .WithReference(borrowerDb, Consts.DefaultDatabase)
            .WaitFor(borrowerDb)
            .WaitFor(kafka)
            .WithHttpCommand(
                path: "/api/cqrs/v1/borrowers",
                displayName: "Register a test book borrwer",
                commandOptions: new HttpCommandOptions()
                {
                    Description = "",
                    PrepareRequest = (context) =>
                    {
                        context.Request.Content = JsonContent.Create(new
                        {
                            Id = "019557EB-49D7-7007-BEDE-5F22B35963D0",
                            Name = "John Doe",
                            Address = "123 Main St",
                            PhoneNumber = "555-555-5555",
                            Email = "john.doe@microservice-patterns.com"
                        });
                        return Task.CompletedTask;
                    },
                    GetCommandResult = GetCommandResult,
                    IconName = "PersonAdd",
                    IsHighlighted = true
                }
            );

        var borrowingDb = postgres.AddDefaultDatabase<Projects.CQRS_Library_BorrowingService>();
        var borrowingService = builder.AddProjectWithPostfix<Projects.CQRS_Library_BorrowingService>()
            .WithEnvironment(Consts.Env_EventPublishingTopics, GetTopicName<Projects.CQRS_Library_BorrowingService>())
            .WithReference(kafka)
            .WithReference(borrowingDb, Consts.DefaultDatabase)
            .WaitFor(borrowingDb)
            .WaitFor(kafka)
            .WithHttpCommand(
                path: "/api/cqrs/v1/borrowings",
                displayName: "Borrow the test book",
                commandOptions: new HttpCommandOptions()
                {
                    Description = "",
                    PrepareRequest = (context) =>
                    {
                        context.Request.Content = JsonContent.Create(new
                        {
                            Id = "00000000-0000-0000-0000-000000000000",
                            BorrowerId = "019557EB-49D7-7007-BEDE-5F22B35963D0",
                            BookId = "019557EE-978C-7CE8-9152-73637B6DC9F5",
                            ValidUntil = DateTime.UtcNow.AddDays(14),
                        });
                        return Task.CompletedTask;
                    },
                    GetCommandResult = GetCommandResult,
                    IconName = "ReceiptAdd",
                    IsHighlighted = true
                }
            );

        var borrowingHistoryDb = postgres.AddDefaultDatabase<Projects.CQRS_Library_BorrowingHistoryService>();
        var borrowingHistoryService = builder.AddProjectWithPostfix<Projects.CQRS_Library_BorrowingHistoryService>()
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
            .WaitFor(kafka)
            .WithHttpCommand(
                path: "/api/cqrs/v1/history/items",
                displayName: "List borrowed books",
                commandOptions: new HttpCommandOptions()
                {
                    Description = "",
                    PrepareRequest = (context) =>
                    {
                        context.Request.Method = HttpMethod.Get;
                        return Task.CompletedTask;
                    },
                    GetCommandResult = GetCommandResult,
                    IconName = "BookTemplate",
                    IsHighlighted = true
                }
            );

        bookService.WithParentRelationship(borrowingHistoryService);
        borrowerService.WithParentRelationship(borrowingHistoryService);
        borrowingService.WithParentRelationship(borrowingHistoryService);
        #endregion CQRS Library

        #region Saga Online Store - Choreography
        var sagaOrderDb = postgres.AddDefaultDatabase<Projects.Saga_OnlineStore_OrderService>();
        var sagaOrderService = builder.AddProjectWithPostfix<Projects.Saga_OnlineStore_OrderService>()
            .WithEnvironment(Consts.Env_EventPublishingTopics, GetTopicName<Projects.Saga_OnlineStore_OrderService>())
            .WithEnvironment(Consts.Env_EventConsumingTopics,
                string.Join(',',
                    GetTopicName<Projects.Saga_OnlineStore_InventoryService>(),
                    GetTopicName<Projects.Saga_OnlineStore_PaymentService>()
                    )
                )
            .WithReference(kafka)
            .WithReference(sagaOrderDb, Consts.DefaultDatabase)
            .WaitFor(sagaOrderDb)
            .WaitFor(kafka)
            .WithHttpCommand(
            path: "/api/saga/v1/orders",
            displayName: "Place a new order",
            commandOptions: new HttpCommandOptions()
            {
                Description = "",
                PrepareRequest = (context) =>
                {   context.Request.Content = new StringContent(
                        """
                        {
                          "orderId": "00000000-0000-0000-0000-000000000000",
                          "customerId": "8A168D02-E235-41F5-A22C-DBDEF71A6D0A",
                          "customerName": "John Doe",
                          "customerPhone": "+1234567890",
                          "shippingAddress": "123 Main St, Springfield, IL 62701",
                          "paymentCardNumber": "1234567890123456",
                          "paymentCardName": "John Doe",
                          "paymentCardExpiration": "12/24",
                          "paymentCardCvv": "123",
                          "items": [
                            {
                              "productId": "01957eea-c993-714f-bf4a-ca767716031d",
                              "quantity": 1,
                              "unitPrice": 9.99
                            }
                          ]
                        }
                        """, System.Text.Encoding.UTF8, "application/json");
                    return Task.CompletedTask;
                },
                GetCommandResult = GetCommandResult,
                IconName = "BoxArrowUp",
                IsHighlighted = true
            });

        var sagaCatalogDb = postgres.AddDefaultDatabase<Projects.Saga_OnlineStore_CatalogService>();
        var sagaCatalogService = builder.AddProjectWithPostfix<Projects.Saga_OnlineStore_CatalogService>()
            .WithEnvironment(Consts.Env_EventPublishingTopics, GetTopicName<Projects.Saga_OnlineStore_CatalogService>())
            .WithReference(kafka)
            .WithReference(sagaCatalogDb, Consts.DefaultDatabase)
            .WaitFor(sagaCatalogDb)
            .WaitFor(kafka)
            .WithHttpCommand(
            path: "/api/saga/v1/products",
            displayName: "Register demo product",
            commandOptions: new HttpCommandOptions()
            {
                Description = "",
                PrepareRequest = (context) =>
                {
                    context.Request.Content = JsonContent.Create(new
                    {
                        Id = "01957eea-c993-714f-bf4a-ca767716031d",
                        Name = "Demo Product",
                        Description = "Demo Description",
                        Price = random.Next(1, 100000)
                    });
                    return Task.CompletedTask;
                },
                GetCommandResult = GetCommandResult,
                IconName = "AddCircle",
                IsHighlighted = true
            });

        var sagaInventoryDb = postgres.AddDefaultDatabase<Projects.Saga_OnlineStore_InventoryService>();
        var sagaInventoryService = builder.AddProjectWithPostfix<Projects.Saga_OnlineStore_InventoryService>()
            .WithEnvironment(Consts.Env_EventPublishingTopics, GetTopicName<Projects.Saga_OnlineStore_InventoryService>())
            .WithEnvironment(Consts.Env_EventConsumingTopics,
                string.Join(',',
                    GetTopicName<Projects.Saga_OnlineStore_CatalogService>(),
                    GetTopicName<Projects.Saga_OnlineStore_OrderService>(),
                    GetTopicName<Projects.Saga_OnlineStore_PaymentService>()
                    )
                )
            .WithReference(kafka)
            .WithReference(sagaInventoryDb, Consts.DefaultDatabase)
            .WaitFor(sagaInventoryDb)
            .WaitFor(kafka)
            .WithHttpCommand(
            path: "/api/saga/v1/inventory/items/01957eea-c993-714f-bf4a-ca767716031d/restock",
            displayName: "Add 10 demo products to inventory",
            commandOptions: new HttpCommandOptions()
            {
                Description = "",
                PrepareRequest = (context) =>
                {
                    context.Request.Method = HttpMethod.Put;
                    context.Request.Content = JsonContent.Create(new
                    {
                        Quantity = 10
                    });
                    return Task.CompletedTask;
                },
                GetCommandResult = GetCommandResult,
                IconName = "ReceiptAdd",
                IsHighlighted = true
            })
            .WithParentRelationship(sagaOrderService);

        var sagaBankCardDb = postgres.AddDefaultDatabase<Projects.Saga_OnlineStore_PaymentService>();
        var sagaBankCardService = builder.AddProjectWithPostfix<Projects.Saga_OnlineStore_PaymentService>()
            .WithEnvironment(Consts.Env_EventPublishingTopics, GetTopicName<Projects.Saga_OnlineStore_PaymentService>())
            .WithEnvironment(Consts.Env_EventConsumingTopics,
                string.Join(',',
                    GetTopicName<Projects.Saga_OnlineStore_InventoryService>()
                    )
                )
            .WithReference(kafka)
            .WithReference(sagaBankCardDb, Consts.DefaultDatabase)
            .WaitFor(sagaBankCardDb)
            .WaitFor(kafka)
            .WithHttpCommand(
            path: "/api/saga/v1/cards",
            displayName: "Register demo bank card to payment service",
            commandOptions: new HttpCommandOptions()
            {
                Description = "",
                PrepareRequest = (context) =>
                {
                    context.Request.Content = JsonContent.Create(new
                    {
                        Id = "53974C76-BC26-44CE-9DB2-AE3EBF095A28",
                        CardNumber = "1234567890123456",
                        ExpirationDate = "12/30",
                        CardHolderName = "Test Card Holder",
                        Cvv = "123"
                    });
                    return Task.CompletedTask;
                },
                GetCommandResult = GetCommandResult,
                IconName = "CardUi",
                IsHighlighted = true
            })
            .WithHttpCommand(
            path: "/api/saga/v1/cards/53974C76-BC26-44CE-9DB2-AE3EBF095A28/deposit",
            displayName: "Deposit to demo bank card",
            commandOptions: new HttpCommandOptions()
            {
                Description = "",
                PrepareRequest = (context) =>
                {
                    context.Request.Method = HttpMethod.Put;
                    context.Request.Content = JsonContent.Create(new
                    {
                        Amount = 999999
                    });
                    return Task.CompletedTask;
                },
                GetCommandResult = GetCommandResult,
                IconName = "WalletCreditCard",
                IsHighlighted = true
            });


        sagaBankCardService.WithParentRelationship(sagaOrderService);
        sagaCatalogService.WithParentRelationship(sagaOrderService);
        sagaInventoryService.WithParentRelationship(sagaOrderService);
        #endregion

        #region Saga Trip Planner - Orchestration
        var sagaHotelDb = postgres.AddDefaultDatabase<Projects.Saga_TripPlanner_HotelService>();
        var sagaHotelService = builder.AddProjectWithPostfix<Projects.Saga_TripPlanner_HotelService>()
            .WithEnvironment(Consts.Env_EventPublishingTopics, GetTopicName<Projects.Saga_TripPlanner_HotelService>())
            .WithEnvironment(Consts.Env_EventConsumingTopics,
                string.Join(',',
                    GetTopicName<Projects.Saga_TripPlanner_TripPlanningService>()
                    )
                )
            .WithReference(kafka)
            .WithReference(sagaHotelDb, Consts.DefaultDatabase)
            .WaitFor(sagaHotelDb)
            .WaitFor(kafka);

        var sagaTicketlDb = postgres.AddDefaultDatabase<Projects.Saga_TripPlanner_TicketService>();
        var sagaTicketService = builder.AddProjectWithPostfix<Projects.Saga_TripPlanner_TicketService>()
            .WithEnvironment(Consts.Env_EventPublishingTopics, GetTopicName<Projects.Saga_TripPlanner_TicketService>())
            .WithEnvironment(Consts.Env_EventConsumingTopics,
                string.Join(',',
                    GetTopicName<Projects.Saga_TripPlanner_TripPlanningService>()
                    )
                )
            .WithReference(kafka)
            .WithReference(sagaTicketlDb, Consts.DefaultDatabase)
            .WaitFor(sagaTicketlDb)
            .WaitFor(kafka);

        var sagaPaymentDb = postgres.AddDefaultDatabase<Projects.Saga_TripPlanner_PaymentService>();
        var sagaPaymentService = builder.AddProjectWithPostfix<Projects.Saga_TripPlanner_PaymentService>()
            .WithEnvironment(Consts.Env_EventPublishingTopics, GetTopicName<Projects.Saga_TripPlanner_PaymentService>())
            .WithEnvironment(Consts.Env_EventConsumingTopics,
                string.Join(',',
                    GetTopicName<Projects.Saga_TripPlanner_TripPlanningService>()
                    )
                )
            .WithReference(kafka)
            .WithReference(sagaPaymentDb, Consts.DefaultDatabase)
            .WaitFor(sagaPaymentDb)
            .WaitFor(kafka);

        var sagaTripPlanningDb = postgres.AddDefaultDatabase<Projects.Saga_TripPlanner_TripPlanningService>();
        var sagaTripPlanningService = builder.AddProjectWithPostfix<Projects.Saga_TripPlanner_TripPlanningService>()
            .WithEnvironment(Consts.Env_EventPublishingTopics, GetTopicName<Projects.Saga_TripPlanner_TripPlanningService>())
            .WithEnvironment(Consts.Env_EventConsumingTopics,
                string.Join(',',
                    GetTopicName<Projects.Saga_TripPlanner_TripPlanningService>()
                    )
                )
            .WithReference(kafka)
            .WithReference(sagaTripPlanningDb, Consts.DefaultDatabase)
            .WithReference(sagaPaymentService)
            .WithReference(sagaTicketService)
            .WithReference(sagaHotelService)
            .WaitFor(sagaTripPlanningDb)
            .WaitFor(kafka);

        sagaHotelService.WithParentRelationship(sagaTripPlanningService);
        sagaTicketService.WithParentRelationship(sagaTripPlanningService);
        sagaPaymentService.WithParentRelationship(sagaTripPlanningService);
        #endregion

        #region Event Sourcing Account

        var esAccountDb = postgres.AddDefaultDatabase<Projects.EventSourcing_Banking_AccountService>();
        var esAccountService = builder.AddProjectWithPostfix<Projects.EventSourcing_Banking_AccountService>()
            .WithReference(esAccountDb, Consts.DefaultDatabase)
            .WaitFor(esAccountDb)
            .WithHttpCommand(
            path: "/api/eventsourcing/v1/accounts",
            displayName: "Register an account with random balance",
            commandOptions: new HttpCommandOptions()
            {
                Description = "",
                PrepareRequest = (context) =>
                {
                    context.Request.Content = JsonContent.Create(new
                    {
                        Id = Guid.Empty,
                        AccountNumber = GenerateRandomNumericString(10),
                        Currency = "USD",
                        Balance = random.Next(),
                        CreditLimit = 0
                    });
                    return Task.CompletedTask;
                },
                GetCommandResult = GetCommandResult,
                IconName = "BookAdd",
                IsHighlighted = true
            }
        );

        var esNotificationService = builder.AddProjectWithPostfix<Projects.EventSourcing_NotificationService>()
            .WithReference(esAccountDb, connectionName: "EventSourcingDb")
            .WaitFor(esAccountService);

        esNotificationService.WithParentRelationship(esAccountService);

        #endregion

        #region Transactional Outbox Account
        var outboxAccountDb = postgres.AddDefaultDatabase<Projects.TransactionalOutbox_Banking_AccountService>();

        var outboxAccountService = builder.AddProjectWithPostfix<Projects.TransactionalOutbox_Banking_AccountService>()
            .WithEnvironment(Consts.Env_EventPublishingTopics, GetTopicName<Projects.TransactionalOutbox_Banking_AccountService>())
            .WithReference(kafka)
            .WithReference(outboxAccountDb, Consts.DefaultDatabase)
            //.WithReference(debezium)
            .WaitFor(outboxAccountDb)
            //.WaitFor(debezium)
            .WaitFor(kafka)
            .WithHttpCommand(
        path: "/api/outbox/v1/accounts",
        displayName: "Register new account",
        commandOptions: new HttpCommandOptions()
        {
            Description = """            
                Register a new account.            
                """,
            PrepareRequest = (context) =>
            {
                context.Request.Content = JsonContent.Create(new
                {
                    Id = "00000000-0000-0000-0000-000000000000",
                    AccountNumber = "1234567890",
                    Currency = "USD",
                    Balance = 0,
                    CreditLimit = 0
                });
                return Task.CompletedTask;
            },
            IconName = "Send",
            IsHighlighted = true
        });

        var outboxConsumingService = builder.AddProjectWithPostfix<Projects.TransactionalOutbox_MessageConsumingService>()
            .WithReference(kafka)
            .WithEnvironment(Consts.Env_EventConsumingTopics, GetTopicName<Projects.TransactionalOutbox_Banking_AccountService>())
            .WaitFor(kafka);

        outboxConsumingService.WithParentRelationship(outboxAccountService);

        #endregion

        #region Idempotent Consumer Catalog
        var idempotentCatalogDb = postgres.AddDefaultDatabase<Projects.IdempotentConsumer_CatalogService>();
        var idempotentCatalogService = builder.AddProjectWithPostfix<Projects.IdempotentConsumer_CatalogService>()
            .WithReference(idempotentCatalogDb, Consts.DefaultDatabase)
            .WaitFor(idempotentCatalogDb);
        #endregion

        #region WebHook 
        var webhookDeliveryServiceDb = postgres.AddDefaultDatabase<Projects.WebHook_DeliveryService>();

        var webHookDeliveryService = builder.AddProjectWithPostfix<Projects.WebHook_DeliveryService>()
            .WithReference(webhookDeliveryServiceDb, Consts.DefaultDatabase)
            .WaitFor(webhookDeliveryServiceDb)
            .WithHttpCommand(
        path: "/api/webhook/v1/webhooks/",
        displayName: "Register Test Webhook",
        commandOptions: new HttpCommandOptions()
        {
            Description = """            
                Register a webhook to receive events from the system.            
                """,
            PrepareRequest = (context) =>
            {
                context.Request.Content = JsonContent.Create(new
                {
                    Url = "https://localhost:7392/webhook"
                });
                return Task.CompletedTask;
            },
            IconName = "DocumentLightning",
            IsHighlighted = true
        }).WithHttpCommand(
        path: "/api/webhook/v1/deliveries/",
        displayName: "Send delivery",
        commandOptions: new HttpCommandOptions()
        {
            Description = """            
                Send a delivery and activate the webhook.
                """,
            PrepareRequest = (context) =>
            {
                context.Request.Content = JsonContent.Create(new
                {
                    Id = "00000000-0000-0000-0000-000000000000",
                    Sender = "Sender Name",
                    Receiver = "Receiver Name",
                    SenderAddress = "Sender Address",
                    ReceiverAddress = "Receiver Address",
                    PackageInfo = "Package Info"
                });
                return Task.CompletedTask;
            },
            IconName = "Send",
            IsHighlighted = true
        });

        var webhookDispatchService = builder.AddProjectWithPostfix<Projects.WebHook_DeliveryService_DispatchService>()
            .WithReference(webHookDeliveryService)
            .WithReference(webhookDeliveryServiceDb, Consts.DefaultDatabase)
            .WaitFor(webHookDeliveryService);

        var webhookEventConsumer = builder.AddProjectWithPostfix<Projects.WebHook_DeliveryService_EventConsumer>()
            .WithReference(webHookDeliveryService)
            .WaitFor(webHookDeliveryService);

        webhookDispatchService.WithParentRelationship(webHookDeliveryService);
        webhookEventConsumer.WithParentRelationship(webHookDeliveryService);

        #endregion

        #region MCP Servers
        var mcpLibraryServer = builder.AddProjectWithPostfix<Projects.Mcp_CQRS_Library_McpServer>()
            .WithReference(bookService)
            .WithReference(borrowerService)
            .WithReference(borrowingService)
            .WithReference(borrowingHistoryService);
        #endregion

        #region YARP Gateway
        var yarp = builder.AddYarp("gateway")
            .WithHostPort(9999)
            .WithConfiguration(yarp =>
        {
            yarp.AddRoute("/api/cqrs/v1/books/{**catch-all}", bookService);
            yarp.AddRoute("/api/cqrs/v1/borrowers/{**catch-all}", borrowerService);
            yarp.AddRoute("/api/cqrs/v1/borrowings/{**catch-all}", borrowingService);
            yarp.AddRoute("/api/cqrs/v1/history/{**catch-all}", borrowingHistoryService);

            yarp.AddRoute("/api/saga/v1/orders/{**catch-all}", sagaOrderService);
            yarp.AddRoute("/api/saga/v1/products/{**catch-all}", sagaCatalogService);
            yarp.AddRoute("/api/saga/v1/inventory/{**catch-all}", sagaInventoryService);
            yarp.AddRoute("/api/saga/v1/cards/{**catch-all}", sagaBankCardService);
            yarp.AddRoute("/api/saga/v1/trips/{**catch-all}", sagaTripPlanningService);
            yarp.AddRoute("/api/saga/v1/hotels/{**catch-all}", sagaHotelService);
            yarp.AddRoute("/api/saga/v1/tickets/{**catch-all}", sagaTicketService);
            yarp.AddRoute("/api/saga/v1/payments/{**catch-all}", sagaPaymentService);

            yarp.AddRoute("/api/eventsourcing/v1/accounts/{**catch-all}", esAccountService);

            yarp.AddRoute("/api/outbox/v1/accounts/{**catch-all}", outboxAccountService);

            yarp.AddRoute("/api/idempotent/v1/products/{**catch-all}", idempotentCatalogService);
            
            yarp.AddRoute("/api/webhook/v1/{**catch-all}", webHookDeliveryService);

            yarp.AddRoute("/mcp/library/{**catch-all}", mcpLibraryServer);

        });
        #endregion

        return builder;
    }

    // remove this method when you don't want Yarp to access HTTPS services with invalid certificates
    private static YarpRoute AddRoute(this IYarpConfigurationBuilder yarp, string path, IResourceBuilder<ProjectResource> resource)
    {
        var serviceCluster = yarp.AddCluster(resource).WithHttpClientConfig(
            new Yarp.ReverseProxy.Configuration.HttpClientConfig() { DangerousAcceptAnyServerCertificate = GatewayDangerousAcceptAnyServerCertificate }
            );
        return yarp.AddRoute(path, serviceCluster);
    }

    private static async Task<ExecuteCommandResult> GetCommandResult(HttpCommandResultContext context)
    {
        var logger = context.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(GetCommandResult));

        var response = context.Response;
        if (response is not null)
        {
            var content = await response.Content.ReadAsStringAsync(context.CancellationToken);
            logger.LogInformation("Response: {StatusCode} {ReasonPhrase} {Result}", response.StatusCode, response.ReasonPhrase, content);
        }
        else
        {
            logger.LogInformation("No response received.");
        }

        return new ExecuteCommandResult() { Success = true };
    }

    // Uncomment the following method to create Kafka topics automatically when the Kafka server is ready.
    //private static async Task CreateKafkaTopics(ResourceReadyEvent @event, KafkaServerResource kafkaResource, CancellationToken ct)
    //{
    //    var logger = @event.Services.GetRequiredService<ILogger<Program>>();

    //    TopicSpecification[] topics = [
    //        new() { Name = GetTopicName<Projects.CQRS_Library_BookService>(), NumPartitions = 1, ReplicationFactor = 1 },
    //        new() { Name = GetTopicName<Projects.CQRS_Library_BorrowerService>(), NumPartitions = 1, ReplicationFactor = 1 },
    //        new() { Name = GetTopicName<Projects.CQRS_Library_BorrowingService>(), NumPartitions = 1, ReplicationFactor = 1 },
    //        new() { Name = GetTopicName<Projects.Saga_OnlineStore_CatalogService>(), NumPartitions = 1, ReplicationFactor = 1 },
    //        new() { Name = GetTopicName<Projects.Saga_OnlineStore_PaymentService>(), NumPartitions = 1, ReplicationFactor = 1 },
    //        new() { Name = GetTopicName<Projects.Saga_OnlineStore_InventoryService>(), NumPartitions = 1, ReplicationFactor = 1 },
    //        new() { Name = GetTopicName<Projects.Saga_OnlineStore_OrderService>(), NumPartitions = 1, ReplicationFactor = 1 }
    //    ];

    //    logger.LogInformation("Creating topics: {topics} ...", string.Join(", ", topics.Select(t => t.Name).ToArray()));

    //    var connectionString = await kafkaResource.ConnectionStringExpression.GetValueAsync(ct);
    //    using var adminClient = new AdminClientBuilder(new AdminClientConfig()
    //    {
    //        BootstrapServers = connectionString,
    //    }).Build();
    //    try
    //    {
    //        await adminClient.CreateTopicsAsync(topics, new CreateTopicsOptions() { });
    //    }
    //    catch (CreateTopicsException ex)
    //    {
    //        logger.LogError(ex, "An error occurred creating topics");
    //    }
    //}

    private static readonly Random random = new();
    private static string GetTopicName<TProject>(string postfix = "") => $"{typeof(TProject).Name.Replace('_', '-')}{(string.IsNullOrEmpty(postfix) ? "" : $"-{postfix}")}";
    private static string GenerateRandomNumericString(int length) 
    {
        return new string([.. Enumerable.Repeat("0123456789", length).Select(s => s[random.Next(s.Length)])]);
    }
}
