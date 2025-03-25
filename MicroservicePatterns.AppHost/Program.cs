using MicroservicePatterns.AppHost.Extensions;

var builder = DistributedApplication.CreateBuilder(args);

builder.AddApplicationServices();

builder.AddProject<Projects.EventSourcing_Banking_AccountService>("eventsourcing-banking-accountservice");

builder.Build().Run();
