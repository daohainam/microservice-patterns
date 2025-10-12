using BFF.ProductCatalog.Search;
using Microsoft.AspNetCore.Mvc;

namespace BFF.ProductCatalog.SearchService.Apis;
public static class ProductSearchApi
{
    private const int defaultPageSize = 10;
    public static IEndpointRouteBuilder MapSearchApi(this IEndpointRouteBuilder builder)
    {
        builder.MapGroup("/api/bff/v1")
              .MapSearchApi()
              .WithTags("Product Search Api");

        return builder;
    }

    public static RouteGroupBuilder MapSearchApi(this RouteGroupBuilder group)
    {
        var productApiGroup = group.MapGroup("products").WithTags("Product");

        productApiGroup.MapGet("/search", FullTextSearchProducts);

        return group;
    }

    private static async Task<IResult> FullTextSearchProducts([AsParameters] ApiServices apiServices, [FromQuery] string? query, [FromQuery] int page = 1, [FromQuery] int pageSize = defaultPageSize)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return Results.BadRequest("Query parameter is required.");
        }

        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? defaultPageSize : pageSize;

        var searchResponse = await apiServices.Client.SearchAsync<ProductIndexDocument>(s => s
            .From((page - 1) * pageSize)
            .Size(pageSize)
            .Query(q => q
                .QueryString(qs => qs
                    .Query(query)
                )
            ),
            apiServices.CancellationToken
        );

        if (!searchResponse.IsValidResponse)
        {
            return Results.BadRequest("Search failed.");
        }

        var products = searchResponse.Documents.ToList();
        return Results.Ok(products);
    }
}
