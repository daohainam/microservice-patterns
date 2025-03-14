using Aspire.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationTests;
internal static class Extensions
{
    public static HttpClient CreateHttpClient<TProject>(this DistributedApplication app, string? endpointName = default)
    {
        string resourceName = (typeof(TProject).Name.Replace('_', '-'));

        return app.CreateHttpClient(resourceName, endpointName);
    }

    public static HttpClient CreateHttpClientWithPostfix<TProject>(this DistributedApplication app, string postfix, string? endpointName = default)
    {
        string resourceName = $"{(typeof(TProject).Name.Replace('_', '-'))}-{postfix}";

        return app.CreateHttpClient(resourceName, endpointName);
    }

    public static Task WaitForResourceAsync<TProject>(this ResourceNotificationService service, string? targetState = null, CancellationToken cancellationToken = default)
    {
        string resourceName = (typeof(TProject).Name.Replace('_', '-'));

        return service.WaitForResourceAsync(resourceName, targetState, cancellationToken);
    }
}
