namespace CQRS.Library.BookService.Bootstraping;
public static class ApplicationServiceExtensions
{
    public static IHostApplicationBuilder AddApplicationServices(this IHostApplicationBuilder builder)
    {
        builder.AddServiceDefaults();
        builder.Services.AddOpenApi();
        builder.AddNpgsqlDbContext<BookDbContext>(Consts.DefaultDatabase);
        builder.AddKafkaEventPublisher("kafka");
        builder.Services.AddKafkaEventPublisher("cqrs-library-book");

        return builder;
    }
}
