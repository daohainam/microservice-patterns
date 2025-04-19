using EventSourcing.NotificationService;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddHostedService<Worker>();

builder.Services.AddSingleton(new NotificationServiceOptions() { 
    ConnectionString = builder.Configuration.GetConnectionString("EventSourcingDb")!
});

var host = builder.Build();
host.Run();
