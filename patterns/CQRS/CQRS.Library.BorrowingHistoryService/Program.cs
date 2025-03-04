using CQRS.Library.BorrowingHistoryService.Apis;
using CQRS.Library.BorrowingHistoryService.Bootstraping;

var builder = WebApplication.CreateBuilder(args);

builder.AddApplicationServices();

var app = builder.Build();

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.MapBorrowingHistoryApi();

await app.MigrateApiDbContextAsync();

app.Run();
