using CQRS.Library.BorrowerService.Apis;
using CQRS.Library.BorrowerService.Bootstraping;

var builder = WebApplication.CreateBuilder(args);

builder.AddApplicationServices();

var app = builder.Build();

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.MapBorrowerApi();

await app.MigrateApiDbContextAsync();

app.Run();
