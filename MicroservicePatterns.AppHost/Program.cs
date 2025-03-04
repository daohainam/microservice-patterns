using MicroservicePatterns.AppHost.Extensions;

var builder = DistributedApplication.CreateBuilder(args);

builder.AddApplicationServices();

builder.AddProject<Projects.Saga_OnlineStore_CatalogService>("saga-onlinestore-catalogservice");

builder.AddProject<Projects.Saga_OnlineStore_InventoryService>("saga-onlinestore-inventoryservice");

builder.Build().Run();
