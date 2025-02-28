using CQRS.Library.BorrowerApi.Infrastructure.Entity;

namespace CQRS.Library.BorrowerApi.Apis;
public static class BorrowerApi
{
    public static IEndpointRouteBuilder MapBorrowerApi(this IEndpointRouteBuilder builder)
    {
        builder.MapGroup("/api/cqrs/v1")
              .MapBorrowerApi()
              .WithTags("Borrower Api");

        return builder;
    }

    public static RouteGroupBuilder MapBorrowerApi(this RouteGroupBuilder group)
    {
        group.MapGet("borrowers", async ([AsParameters] ApiServices services) =>
        {
            return await services.DbContext.Borrowers.ToListAsync();
        });

        group.MapGet("borrowers/{id:guid}", async ([AsParameters] ApiServices services, Guid id) =>
        {
            return await services.DbContext.Borrowers.FindAsync(id);
        });

        group.MapPost("borrowers", async ([AsParameters] ApiServices services, Borrower borrower) =>
        {
            services.DbContext.Borrowers.Add(borrower);
            await services.DbContext.SaveChangesAsync();
            return borrower;
        });

        group.MapPut("borrowers/{id:guid}", async ([AsParameters] ApiServices services, Guid id, Borrower borrower) =>
        {
            if (id != borrower.Id)
            {
                throw new InvalidOperationException("Id mismatch");
            }

            services.DbContext.Borrowers.Update(borrower);
            await services.DbContext.SaveChangesAsync();
            return borrower;
        });

        return group;
    }
}
