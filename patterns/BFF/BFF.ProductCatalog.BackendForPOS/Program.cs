var builder = WebApplication.CreateBuilder(args);
builder.AddApplicationServices();

var app = builder.Build();

app.MapDefaultEndpoints();
app.MapCatalogApi();

app.Run();
