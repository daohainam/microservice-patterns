using CQRS.Library.BorrowerService.Apis;
using CQRS.Library.BorrowerService.Bootstraping;
using Microsoft.Extensions.Hosting;
using System.Threading;

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

await app.MigrateDbContextAsync<BorrowerDbContext>();

app.Run();
