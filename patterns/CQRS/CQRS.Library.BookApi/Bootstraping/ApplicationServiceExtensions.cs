namespace CQRS.Library.BookApi.Bootstraping;
public static class ApplicationServiceExtensions
{
    public static IHostApplicationBuilder AddApplicationServices(this IHostApplicationBuilder builder)
    {
        builder.AddServiceDefaults();
        builder.Services.AddOpenApi();
        builder.AddNpgsqlDbContext<BookDbContext>("cqrs-book-db");
        builder.AddKafkaEventPublisher("kafka");
        builder.Services.AddKafkaEventPublisher("book");

        return builder;
    }
}
