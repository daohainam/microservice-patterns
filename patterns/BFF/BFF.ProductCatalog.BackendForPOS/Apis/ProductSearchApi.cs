using BFF.ProductCatalog.BackendForPOS;
using BFF.ProductCatalog.BackendForPOS.Mappers;
using BFF.ProductCatalog.BackendForPOS.Models;
using BFF.ProductCatalog.Search;
using Microsoft.AspNetCore.Http.HttpResults;
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

        productApiGroup.MapGet("/", SearchProducts);

        return group;
    }

    private static async Task<Results<Ok<List<Product>>, BadRequest, BadRequest<string>>> SearchProducts([AsParameters] ApiServices apiServices, 
        [FromQuery] string? query, [FromQuery(Name = "cat")] Guid? categoryId, [FromQuery(Name = "brand")] Guid? brandId,
        [FromQuery] int page = 1, [FromQuery] int pageSize = defaultPageSize)
    {
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
            return TypedResults.BadRequest("Search failed.");
        }

        var products = searchResponse.Documents.SelectMany(doc => ProductMapper.Map(doc)).ToList();

        return TypedResults.Ok(products);
    }
}
