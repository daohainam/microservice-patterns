using MicroservicePatterns.AppHost.Extensions;

var builder = DistributedApplication.CreateBuilder(args);

builder.AddApplicationServices();

builder.AddProject<Projects.Saga_OnlineStore_BankCardService>("saga-onlinestore-creditcardservice");

builder.Build().Run();
