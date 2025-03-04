using CQRS.Library.BookService.Apis;
using CQRS.Library.BookService.Bootstraping;

var builder = WebApplication.CreateBuilder(args);

builder.AddApplicationServices();

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.MapBorrowerApi();

await app.MigrateApiDbContextAsync();

app.Run();
