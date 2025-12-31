using MicroservicePatterns.Shared;
using WebHook.DeliveryService.DispatchService;
using WebHook.DeliveryService.Infrastructure.Data;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddHttpClient();
builder.Services.AddHostedService<Worker>();

builder.AddNpgsqlDbContext<DeliveryServiceDbContext>(Consts.DefaultDatabase);

var host = builder.Build();
host.Run();
