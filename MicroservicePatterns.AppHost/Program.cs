using MicroservicePatterns.AppHost.Extensions;

var builder = DistributedApplication.CreateBuilder(args);

builder.AddApplicationServices();

builder.AddProject<Projects.Saga_TripPlanner_HotelService>("saga-tripplanner-hotelservice");

builder.AddProject<Projects.Saga_TripPlanner_TicketService>("saga-tripplanner-ticketservice");

builder.AddProject<Projects.Saga_TripPlanner_PaymentService>("saga-tripplanner-paymentservice");

builder.AddProject<Projects.Saga_TripPlanner_TripPlanningService>("saga-tripplanner-tripplanningservice");

builder.Build().Run();
