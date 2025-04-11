var builder = WebApplication.CreateBuilder(args);

builder.AddApplicationServices();

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
app.UseHttpsRedirection();
app.MapDeliveryServiceApi();
app.MapWebHookApi();

await app.MigrateDbContextAsync<DeliveryServiceDbContext>();

app.Run();
