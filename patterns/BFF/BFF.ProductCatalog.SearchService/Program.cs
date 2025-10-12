using BFF.ProductCatalog.SearchService.Apis;
using BFF.ProductCatalogService.Bootstraping;

var builder = WebApplication.CreateBuilder(args);

builder.AddApplicationServices();

var app = builder.Build();

app.MapDefaultEndpoints();
app.MapSearchApi();

app.Run();
