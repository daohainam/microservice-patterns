using MicroservicePatterns.AppHost.Extensions;

var builder = DistributedApplication.CreateBuilder(args);

builder.AddApplicationServices();

builder.AddProject<Projects.CQRS_Library_OrderingHistoryApi>("cqrs-library-orderinghistoryapi");

builder.AddProject<Projects.CQRS_Library_BorrowingHistoryApi>("cqrs-library-borrowinghistoryapi");

builder.Build().Run();
