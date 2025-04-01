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
await app.MigrateDbContextAsync<OutboxDbContext>();


app.Run();
