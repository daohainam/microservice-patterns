using CloudNative.CloudEvents;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

var app = builder.Build();
app.MapDefaultEndpoints();

app.MapPost("/webhook", (ILogger<Program> logger, [FromHeader(Name = "X-Key")]string secretKey, CloudEvent cloudEvent) =>
{

    logger.LogInformation("Webhook received (X-Key={secketKey}): {m}", secretKey, cloudEvent.Data);

    return Results.Ok();
});

app.Run();

