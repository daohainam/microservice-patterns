using BFF.ProductCatalog.BackendForPOS;
using BFF.ProductCatalog.BackendForPOS.Bootstraping;

var builder = WebApplication.CreateBuilder(args);
builder.AddApplicationServices();

var app = builder.Build();

app.MapDefaultEndpoints();

app.UseHttpsRedirection();
app.Run();
