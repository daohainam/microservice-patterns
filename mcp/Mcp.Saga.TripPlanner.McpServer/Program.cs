using Mcp.Saga.TripPlanner.McpServer;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services
    .AddMcpServer()
    .WithHttpTransport()
    .WithToolsFromAssembly(typeof(TripPlanningTool).Assembly);

builder.Services.AddHttpClient("tripplanning",
    static client => client.BaseAddress = new("https+http://Saga-TripPlanner-TripPlanningService"));

builder.Services.AddSingleton<ITripPlanningService>((services) => new TripPlanningService(
    services.GetRequiredService<IHttpClientFactory>().CreateClient("tripplanning")
    ));

var app = builder.Build();

app.MapMcp();
app.MapDefaultEndpoints();

app.Run();
