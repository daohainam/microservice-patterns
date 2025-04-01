using MicroservicePatterns.AppHost.Extensions;

var builder = DistributedApplication.CreateBuilder(args);

builder.AddApplicationServices();

builder.AddProject<Projects.TransactionalOutbox_Banking_AccountService>("transactionaloutbox-banking-accountservice");

builder.Build().Run();
