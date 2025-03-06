namespace MicroservicePatterns.Shared;
public static class ResourceExtensions
{
    public static IResourceBuilder<PostgresDatabaseResource> AddDefaultDatabase<TProject>(this IResourceBuilder<PostgresServerResource> builder)
    {
        return builder.AddDatabase($"{typeof(TProject).Name.Replace('_', '-')}-Db");
    }

    public static IResourceBuilder<ProjectResource> AddProject<TProject>(this IDistributedApplicationBuilder builder) where TProject : IProjectMetadata, new()
    {
        return builder.AddProject<TProject>(typeof(TProject).Name.Replace('_', '-'));
    }

}
