using MicroservicePatterns.AppHost.Extensions;

var builder = DistributedApplication.CreateBuilder(args);

builder.AddApplicationServices();

builder.AddProject<Projects.BFF_ProductCatalog_ReadSideSyncService>("bff-productcatalog-readsidesyncservice");

builder.Build().Run();
