using TransactionalOutbox.Banking.AccountService.Debezium;

var builder = WebApplication.CreateBuilder(args);

builder.AddApplicationServices();

var app = builder.Build();

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.MapAccountApi();

await app.MigrateDbContextAsync<AccountDbContext>();
await app.MigrateOutboxDbContextAsync();

await RegisterDebeziumPublisherAsync(app);

app.Run();

async Task RegisterDebeziumPublisherAsync(WebApplication app)
{
    var httpClient = app.Services.GetRequiredService<IHttpClientFactory>().CreateClient("debezium");

    if (httpClient == null)
    {
        app.Logger.LogWarning("debezium client not registered, debezium publishing skiped");
    }
    else
    {
        var connectionStringBuilder = new NpgsqlConnectionStringBuilder(app.Configuration.GetConnectionString(Consts.DefaultDatabase));

        var response = await httpClient.PostAsJsonAsync("/connectors", new ConnectorRegistration() { 
            Name = "account-service-connector",
            Config = new ConnectorConfig()
            {
                DatabaseHostname = connectionStringBuilder.Host!,
                DatabaseName = connectionStringBuilder.Database!,
                DatabasePassword = connectionStringBuilder.Password!,
                DatabasePort = connectionStringBuilder.Port.ToString(),
                DatabaseUser = connectionStringBuilder.Username!,
                DatabaseServerName = "banking",
                TopicPrefix = "banking-account-01",
                KafkaHistoryBootstrapServers = app.Configuration.GetConnectionString("kafka")!
            }
        });

        app.Logger.LogInformation("Connector registration sent, response: {r}-{s}", response.StatusCode, response.ReasonPhrase);
    }
}