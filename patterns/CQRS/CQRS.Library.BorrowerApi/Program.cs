using CQRS.Library.BorrowerApi.Infrastructure.Entity;
using EventBus.Kafka;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddOpenApi();

builder.AddNpgsqlDbContext<BorrowerDbContext>("cqrs-borrower");
builder.AddKafkaEventPublisher("kafka");

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.Run();
