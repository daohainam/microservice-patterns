namespace TransactionalOutbox.Banking.AccountService.Bootstraping;
public static class ApplicationServiceExtensions
{
    public static IHostApplicationBuilder AddApplicationServices(this IHostApplicationBuilder builder)
    {
        builder.AddServiceDefaults();
        builder.Services.AddOpenApi();

        builder.AddNpgsqlDbContext<AccountDbContext>(Consts.DefaultDatabase);
        builder.AddTransactionalOutbox(Consts.DefaultDatabase);

        if (builder.Configuration.GetConnectionString(Consts.DefaultDatabase) is string connectionString)
        {
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>(sp =>
            {
                var connection = new NpgsqlConnection(connectionString);

                return new UnitOfWork(connection);
            });
        }
        else
        {
            throw new InvalidOperationException($"Connection string '{Consts.DefaultDatabase}' not found.");
        }

        builder.AddKafkaProducer("kafka");
        var kafkaTopic = builder.Configuration.GetValue<string>(Consts.Env_EventPublishingTopics);
        if (!string.IsNullOrEmpty(kafkaTopic))
        {
            builder.AddKafkaEventPublisher(kafkaTopic);
        }
        else
        {
            builder.Services.AddTransient<IEventPublisher, NullEventPublisher>();
        }

        var debeziumConnectionString = builder.Configuration.GetConnectionString("debezium");
        if (debeziumConnectionString != null)
        {
            builder.Services.AddHttpClient("debezium", client =>
            {
                client.BaseAddress = new Uri(debeziumConnectionString);
            });
        }

        return builder;
    }
}
