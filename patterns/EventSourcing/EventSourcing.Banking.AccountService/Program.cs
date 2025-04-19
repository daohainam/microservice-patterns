using Microsoft.EntityFrameworkCore;

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

await app.MigrateEventStoreDatabaseAsync(postMigration: async (db, cancellationToken) => {
    await db.ExecuteSqlAsync($"CREATE OR REPLACE FUNCTION notify_new_event() RETURNS trigger AS $$\r\nBEGIN\r\n  PERFORM pg_notify('event_channel', row_to_json(NEW)::text);\r\n  RETURN NEW;\r\nEND;\r\n$$ LANGUAGE plpgsql;\r\n\r\nCREATE OR REPLACE TRIGGER new_event_trigger\r\nAFTER INSERT ON \"Events\"\r\nFOR EACH ROW EXECUTE FUNCTION notify_new_event();", cancellationToken: cancellationToken ?? CancellationToken.None);
});

app.Run();
