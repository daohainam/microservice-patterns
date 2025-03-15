using MicroservicePatterns.AppHost.Extensions;

var builder = DistributedApplication.CreateBuilder(args);

builder.AddApplicationServices();

builder.AddProject<Projects.Saga_TripPlanner_HotelService>("saga-tripplanner-hotelservice");

builder.Build().Run();
