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
await app.MigrateOutboxDbContextAsync(postMigration: async (db, cancellationToken) => {
    await db.ExecuteSqlAsync($"CREATE OR REPLACE FUNCTION notify_outbox_change() RETURNS trigger AS $$\r\nBEGIN\r\n  PERFORM pg_notify('outbox_channel', row_to_json(NEW)::text);\r\n  RETURN NEW;\r\nEND;\r\n$$ LANGUAGE plpgsql;\r\n\r\nCREATE OR REPLACE TRIGGER outbox_change_trigger\r\nAFTER INSERT ON \"LogTailingOutboxMessages\"\r\nFOR EACH ROW EXECUTE FUNCTION notify_outbox_change();", cancellationToken: cancellationToken ?? CancellationToken.None);
});

//await RegisterDebeziumPublisherAsync(app);

app.Run();

// we don't use debezium since .NET Aspire doesn't support container to container scenarios
//static async Task RegisterDebeziumPublisherAsync(WebApplication app)
//{
//    var httpClient = app.Services.GetRequiredService<IHttpClientFactory>().CreateClient("debezium");

//    if (httpClient == null)
//    {
//        app.Logger.LogWarning("debezium client not registered, debezium publishing skiped");
//    }
//    else
//    {
//        var connectionStringBuilder = new NpgsqlConnectionStringBuilder(app.Configuration.GetConnectionString(Consts.DefaultDatabase));

//        try
//        {
//            var response = await httpClient.PostAsJsonAsync("/connectors", new ConnectorRegistration()
//            {
//                Name = "account-service-connector",
//                Config = new ConnectorConfig()
//                {
//                    DatabaseHostname = connectionStringBuilder.Host!,
//                    DatabaseName = connectionStringBuilder.Database!,
//                    DatabasePassword = connectionStringBuilder.Password!,
//                    DatabasePort = connectionStringBuilder.Port.ToString(),
//                    DatabaseUser = connectionStringBuilder.Username!,
//                    DatabaseServerName = "banking",
//                    TopicPrefix = "banking-account-01",
//                    KafkaHistoryBootstrapServers = app.Configuration.GetConnectionString("kafka")!
//                }
//            });

//            app.Logger.LogInformation("Connector registration sent, response: {r}-{s}", response.StatusCode, response.ReasonPhrase);

//            response.EnsureSuccessStatusCode();
//        }
//        catch (Exception ex)
//        {
//            app.Logger.LogError(ex, "Failed to register debezium connector");
//        }
//    }
//}