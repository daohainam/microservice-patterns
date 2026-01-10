using Mcp.Saga.OnlineStore.McpServer;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services
    .AddMcpServer()
    .WithHttpTransport()
    .WithToolsFromAssembly(typeof(OrderTool).Assembly);

builder.Services.AddHttpClient("order",
    static client => client.BaseAddress = new("https+http://Saga-OnlineStore-OrderService"));

builder.Services.AddSingleton<IOrderService>((services) => new OrderService(
    services.GetRequiredService<IHttpClientFactory>().CreateClient("order")
    ));

var app = builder.Build();

app.MapMcp();
app.MapDefaultEndpoints();

app.Run();
