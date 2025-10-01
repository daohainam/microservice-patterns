using BFF.ProductCatalogService.Apis;
using BFF.ProductCatalogService.Bootstraping;

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
app.MapCatalogApi();

await app.MigrateDbContextAsync<ProductCatalogDbContext>();

app.Run();
