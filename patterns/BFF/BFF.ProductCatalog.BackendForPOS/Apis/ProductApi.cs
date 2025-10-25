using BFF.ProductCatalog.BackendForPOS.Mappers;
using BFF.ProductCatalog.BackendForPOS.Models;
using BFF.ProductCatalog.Search;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace BFF.ProductCatalog.BackendForPOS.Apis;
public static class ProductSearchApi
{
    private const int defaultPageSize = 10;
    public static IEndpointRouteBuilder MapCatalogApi(this IEndpointRouteBuilder builder)
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

    private static async Task<Results<Ok<List<POSProduct>>, BadRequest, BadRequest<string>>> SearchProducts([AsParameters] ApiServices apiServices, 
        [FromQuery] string? query, [FromQuery(Name = "catId")] Guid? categoryId, [FromQuery(Name = "brandId")] Guid? brandId,
        [FromQuery] int page = 1, [FromQuery] int pageSize = defaultPageSize)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? defaultPageSize : pageSize;

        var searchQuery = new BoolQuery
        {
            Must = [],
            Filter = []
        };

        if (!string.IsNullOrWhiteSpace(query))
        {
            searchQuery.Must.Add(new MultiMatchQuery
            {
                Fields = new Field[] { "name", "description", "brandName", "categoryName", "groupNames" },
                Query = query
            });
        }

        if (categoryId.HasValue)
        {
            searchQuery.Filter.Add(new TermQuery
            {
                Field = "categoryId",
                Value = FieldValue.String(categoryId.Value.ToString())
            });
        }

        if (brandId.HasValue) {             
            searchQuery.Filter.Add(new TermQuery
            {
                Field = "brandId",
                Value = FieldValue.String(brandId.Value.ToString())
            });
        }

        if (searchQuery.Must.Count == 0)
        {
            searchQuery.Must.Add(new MatchAllQuery());
        }

        var request = new SearchRequest<ProductIndexDocument>
        {
            From = (page - 1) * pageSize,
            Size = pageSize,
            Query = searchQuery
        };

        var searchResponse = await apiServices.Client.SearchAsync<ProductIndexDocument>(request, apiServices.CancellationToken);

        if (!searchResponse.IsValidResponse)
        {
            return TypedResults.BadRequest("Search failed.");
        }

        var products = searchResponse.Documents.SelectMany(doc => ProductMapper.Map(doc)).ToList();

        return TypedResults.Ok(products);
    }
}
