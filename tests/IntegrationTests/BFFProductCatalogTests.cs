using Aspire.Hosting;
using BFF.ProductCatalogService.Infrastructure.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationTests.Tests;
public class BFFProductCatalogTests(AppFixture fixture)
{
    private DistributedApplication App => fixture.App;

    private async Task<Product?> Create_Test_Product(HttpClient httpClient)
    {
        var categoryId = Guid.NewGuid();
        var brandId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        var response = await httpClient.PostAsJsonAsync("/api/bff/v1/categories", new Category()
        {
            Id = categoryId,
            Name = "Test Category",
            ParentCategoryId = null,
            Description = "This is a test category",
            UrlSlug = "test-category",
        }, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Act
        response = await httpClient.PostAsJsonAsync("/api/bff/v1/brands", new Brand()
        {
            Id = brandId,
            Name = "Test Brand",
            Description = "This is a test brand",
            UrlSlug = "test-brand",
        }, cancellationToken: TestContext.Current.CancellationToken); 
        
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        response = await httpClient.PostAsJsonAsync("/api/bff/v1/products", new Product()
        {
            Id = productId,
            CategoryId = categoryId,
            BrandId = brandId,
            Name = "Test Product",
            Description = "This is a test product",
            IsActive = true,
            IsDeleted = false,
            UrlSlug = "test-product"
        }, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Act
        response = await httpClient.GetAsync($"/api/bff/v1/products/{productId}", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var product = await response.Content.ReadFromJsonAsync<Product>(cancellationToken: TestContext.Current.CancellationToken);

        return product;
    }

    [Fact]
    public async Task Create_Product_And_Read_Success()
    {
        // Arrange
        var resourceNotificationService = App.Services.GetRequiredService<ResourceNotificationService>();
        await resourceNotificationService.WaitForResourceAsync<Projects.BFF_ProductCatalogService>(KnownResourceStates.Running, cancellationToken: TestContext.Current.CancellationToken).WaitAsync(TimeSpan.FromSeconds(30), TestContext.Current.CancellationToken);

        // Act
        var httpClient = App.CreateHttpClient<Projects.BFF_ProductCatalogService>();

        var product = await Create_Test_Product(httpClient);

        Assert.NotNull(product);
        Assert.Equal("Test Product", product.Name);
        Assert.Equal("This is a test product", product.Description);

        // Set up dimensions
        var dimension1 = new Dimension() { Id = "color", Name = "Color", DefaultValue = "", DisplayType = "color" };
        var dimension2 = new Dimension() { Id = "size", Name = "Size", DefaultValue = "M", Values = [new() { Value = "XS" } , new() { Value = "S" }, new() { Value = "M" }, new() { Value = "L" }, new() { Value = "XL" }] };
        var response = await httpClient.PostAsJsonAsync("/api/bff/v1/dimensions", new[] { dimension1, dimension2 }, cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var dimensions = await response.Content.ReadFromJsonAsync<List<Dimension>>(cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotNull(dimensions);
        Assert.Equal(2, dimensions.Count);
        Assert.Contains(dimensions, d => d.Id == "color");
        Assert.Contains(dimensions, d => d.Id == "size");
        Assert.Empty(dimensions.First(d => d.Id == "color").Values);
        Assert.Equal(5, dimensions.First(d => d.Id == "size").Values.Count);
        Assert.Contains(dimensions.First(d => d.Id == "size").Values, v => v.Value == "XS");
        Assert.Contains(dimensions.First(d => d.Id == "size").Values, v => v.Value == "S");
        Assert.Contains(dimensions.First(d => d.Id == "size").Values, v => v.Value == "M");
        Assert.Contains(dimensions.First(d => d.Id == "size").Values, v => v.Value == "L");
        Assert.Contains(dimensions.First(d => d.Id == "size").Values, v => v.Value == "XL");

        // add dimensions to product
        response = await httpClient.PostAsJsonAsync($"/api/bff/v1/dimensions/{dimensions.First(d => d.Id == "color").Id}/values", new DimensionValue[] {
            new() { DimensionId = "color", Value = "ff0000", DisplayValue = "Red" }, 
            new() { DimensionId = "color", Value = "00ff00", DisplayValue = "Green" },
            new() { DimensionId = "color", Value = "0000ff", DisplayValue = "Blue" },
        }, cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // get dimensions
        response = await httpClient.GetAsync($"/api/bff/v1/dimensions", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        dimensions = await response.Content.ReadFromJsonAsync<List<Dimension>>(cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotNull(dimensions);
        Assert.Equal(2, dimensions.Count);
        Assert.Contains(dimensions, d => d.Id == "color");
        Assert.Equal(3, dimensions.First(d => d.Id == "color").Values.Count);
        Assert.Contains(dimensions.First(d => d.Id == "color").Values, v => v.Value == "ff0000" && v.DisplayValue == "Red");
        Assert.Contains(dimensions.First(d => d.Id == "color").Values, v => v.Value == "00ff00" && v.DisplayValue == "Green");
        Assert.Contains(dimensions.First(d => d.Id == "color").Values, v => v.Value == "0000ff" && v.DisplayValue == "Blue");

        Assert.Contains(dimensions, d => d.Id == "size");
        Assert.Equal(5, dimensions.First(d => d.Id == "size").Values.Count);
        Assert.Contains(dimensions.First(d => d.Id == "size").Values, v => v.Value == "XS");
        Assert.Contains(dimensions.First(d => d.Id == "size").Values, v => v.Value == "S");
        Assert.Contains(dimensions.First(d => d.Id == "size").Values, v => v.Value == "M");
        Assert.Contains(dimensions.First(d => d.Id == "size").Values, v => v.Value == "L");
        Assert.Contains(dimensions.First(d => d.Id == "size").Values, v => v.Value == "XL");

        // add dimensions to product
        response = await httpClient.PostAsJsonAsync($"/api/bff/v1/products/{product.Id}/dimensions", new Dimension[] { new() { Id = "color" }, new() { Id = "size" } }, cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // add variants to product
        var variant1 = new Variant()
        {
            Id = Guid.NewGuid(),
            ProductId = product.Id,
            Sku = "TEST-SKU-001",
            Price = 19.99m,
            Stock = 100,
            BarCode = "1234567890001",
            Description = "This is a test variant",
            IsActive = true,
            IsDeleted = false,
            DimensionValues =
            [
                new() { DimensionId = "color", Value = "ff0000" },
                new() { DimensionId = "size", Value = "M" }
            ]
        };

        var variant2 = new Variant() {
            Id = Guid.NewGuid(),
            ProductId = product.Id,
            Sku = "TEST-SKU-002",
            Price = 21.99m,
            Stock = 50,
            BarCode = "1234567890002",
            Description = "This is another test variant",
            IsActive = true,
            IsDeleted = false,
            DimensionValues =
            [
                new() { DimensionId = "color", Value = "00ff00" },
                new() { DimensionId = "size", Value = "L" }
            ]
        };

        response = await httpClient.PostAsJsonAsync($"/api/bff/v1/products/{product.Id}/variants", new[] { variant1, variant2 }, cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // get product and verify variants
        response = await httpClient.GetAsync($"/api/bff/v1/products/{product.Id}", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        product = await response.Content.ReadFromJsonAsync<Product>(cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotNull(product);
        Assert.Equal(2, product.Variants.Count);
        Assert.Contains(product.Variants, v => v.Id == variant1.Id && v.Sku == "TEST-SKU-001" && v.Price == 19.99m && v.Stock == 100 && v.DimensionValues.Any(dv => dv.DimensionId == "color" && dv.Value == "ff0000") && v.DimensionValues.Any(dv => dv.DimensionId == "size" && dv.Value == "M"));
    }
}
