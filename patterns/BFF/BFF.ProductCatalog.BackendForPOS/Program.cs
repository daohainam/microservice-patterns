using BFF.ProductCatalog.BackendForPOS;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddHttpClient("product-service",
    static client => client.BaseAddress = new("https+http://BFF-ProductCatalog-ProductCatalogService"));
builder.Services.AddHttpClient("product-search-service",
    static client => client.BaseAddress = new("https+http://BFF-ProductCatalog-SearchService"));
builder.Services.AddScoped((services) => new ApiServices(
    services.GetRequiredService<IHttpClientFactory>().CreateClient("product-service"),
    services.GetRequiredService<IHttpClientFactory>().CreateClient("product-search-service")
    ));

var app = builder.Build();

app.MapDefaultEndpoints();

app.UseHttpsRedirection();
app.Run();
