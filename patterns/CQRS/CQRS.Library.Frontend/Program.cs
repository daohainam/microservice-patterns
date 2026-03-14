using CQRS.Library.Frontend.Components;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddMudServices();

builder.Services.AddHttpClient("bookService", client =>
{
    client.BaseAddress = new Uri("https+http://CQRS-Library-BookService");
}).AddServiceDiscovery();

builder.Services.AddHttpClient("borrowerService", client =>
{
    client.BaseAddress = new Uri("https+http://CQRS-Library-BorrowerService");
}).AddServiceDiscovery();

builder.Services.AddHttpClient("borrowingService", client =>
{
    client.BaseAddress = new Uri("https+http://CQRS-Library-BorrowingService");
}).AddServiceDiscovery();

builder.Services.AddHttpClient("borrowingHistoryService", client =>
{
    client.BaseAddress = new Uri("https+http://CQRS-Library-BorrowingHistoryService");
}).AddServiceDiscovery();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found");
app.UseHttpsRedirection();
app.UseAntiforgery();
app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
