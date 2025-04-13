using System.Text.Json;
using WebHook.DeliveryService.DomainEvents;
using WebHook.DeliveryService.Infrastructure.Entity;

namespace WebHook.DeliveryService.Apis;
public static class WebHookApiExtensions
{
    public static IEndpointRouteBuilder MapWebHookApi(this IEndpointRouteBuilder builder)
    {
        builder.MapGroup("/api/webhook/v1")
              .MapWebHookApi()
              .WithTags("WebHook Api");

        return builder;
    }
    public static RouteGroupBuilder MapWebHookApi(this RouteGroupBuilder group)
    {
        group.MapPost("webhooks", WebHookApi.RegisterWebHook);

        group.MapPut("webhooks/{id:guid}/unregister", WebHookApi.UnregisterWebHook);

        return group;
    }
}
public class WebHookApi
{ 
    internal static async Task<Results<Ok<WebHookSubscription>, BadRequest, BadRequest<string>>> RegisterWebHook([AsParameters] ApiServices services, WebHookRegisterRequest webHook)
    {
        if (webHook == null) {
            return TypedResults.BadRequest();
        }

        if (string.IsNullOrWhiteSpace(webHook.Url))
        {
            return TypedResults.BadRequest("Url is required");
        }

        if (!Uri.TryCreate(webHook.Url, UriKind.Absolute, out var uriResult) || (uriResult.Scheme != Uri.UriSchemeHttp && uriResult.Scheme != Uri.UriSchemeHttps))
        {
            return TypedResults.BadRequest("Invalid Url format");
        }

        var webHookSubscription = new WebHookSubscription()
        {
            Id = Guid.CreateVersion7(),
            Url = webHook.Url,
            SecretKey = services.SecretKeyService.GetKey(),
        };

        await services.DbContext.WebHookSubscriptions.AddAsync(webHookSubscription);

        await services.DbContext.SaveChangesAsync();

        return TypedResults.Ok(webHookSubscription);
    }

    internal static async Task<Results<NotFound, Ok>> UnregisterWebHook([AsParameters] ApiServices services, Guid id, WebHookSubscription webHook)
    {
        var webHookSubscription = await services.DbContext.WebHookSubscriptions.Where(wh => wh.Id == id && wh.Url == webHook.Url && wh.SecretKey == webHook.SecretKey).SingleOrDefaultAsync();
        if (webHookSubscription == null)
        {
            return TypedResults.NotFound();
        }

        services.DbContext.Entry(webHookSubscription).State = EntityState.Deleted;

        await services.DbContext.SaveChangesAsync();

        return TypedResults.Ok();
    }
}

public class WebHookRegisterRequest
{
    public string Url { get; set; } = default!;
}
