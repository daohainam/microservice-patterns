using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

var app = builder.Build();
app.MapDefaultEndpoints();

app.MapPost("/webhook", async (ILogger<Program> logger, [FromHeader(Name = "X-Key")]string secretKey, HttpRequest request, Stream body) =>
{
    if (request.ContentLength is null)
    {
        return Results.BadRequest();
    }

    using var reader = new StreamReader(body);
    var content = await reader.ReadToEndAsync();

    logger.LogInformation("Webhook received (X-Key={secketKey}): {m}", secretKey, content);

    return Results.Ok();
});

app.Run();

