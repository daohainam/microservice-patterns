var builder = WebApplication.CreateBuilder(args);

builder.AddApplicationServices();

var app = builder.Build();

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}
app.UseHttpsRedirection();
app.MapCatalogApi();

await app.MigrateDbContextAsync<ProductCatalogDbContext>();
await app.MigrateOutboxDbContextAsync(postMigration: async (db, cancellationToken) => {
    await db.ExecuteSqlAsync($"CREATE OR REPLACE FUNCTION notify_outbox_change() RETURNS trigger AS $$\r\nBEGIN\r\n  PERFORM pg_notify('outbox_channel', row_to_json(NEW)::text);\r\n  RETURN NEW;\r\nEND;\r\n$$ LANGUAGE plpgsql;\r\n\r\nCREATE OR REPLACE TRIGGER outbox_change_trigger\r\nAFTER INSERT ON \"LogTailingOutboxMessages\"\r\nFOR EACH ROW EXECUTE FUNCTION notify_outbox_change();", cancellationToken: cancellationToken ?? CancellationToken.None);
});

app.Run();
