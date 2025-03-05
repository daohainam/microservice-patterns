using CQRS.Library.BorrowingService.Apis;
using CQRS.Library.BorrowingService.Bootstraping;
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
app.MapBorrowingApi();

await app.MigrateDbContextAsync<BorrowingDbContext>();

app.Run();
