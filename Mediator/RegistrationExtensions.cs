using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Mediator;
public static class RegistrationExtensions
{
    public static IServiceCollection AddMediator(this IServiceCollection services, Action<MediatorRegistrationOptions>? action = null)
    {
        var options = new MediatorRegistrationOptions();
        action?.Invoke(options);

        // services.AddTransient(typeof(INotificationHandler<>), typeof(NotificationHandler<>));

        services.RegisterHandlers(options.ServiceAssemblies);
        services.AddSingleton<IMediator, Mediator>();

        return services;
    }

    private static void RegisterHandlers(this IServiceCollection services, IEnumerable<Assembly> serviceAssemblies)
    {
        var assemblies = new List<Assembly>() { Assembly.GetExecutingAssembly() };
        if (serviceAssemblies != null)
        {
            assemblies.AddRange(serviceAssemblies);
        }

        foreach (var assembly in assemblies)
        {
            var notificationTypes = assembly
                .GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && t.GetInterfaces().Any(i =>
                    i.IsGenericType/* && i.GetGenericTypeDefinition().Name == "INotification`1"*/));

            foreach (var notificationType in notificationTypes)
            {
                var handlerInterfaces = notificationType
                    .GetInterfaces()
                    .Where(i => i.IsGenericType && i.GetGenericTypeDefinition().Name == "INotificationHandler`1");

                foreach (var handlerInterface in handlerInterfaces)
                {
                    services.AddTransient(handlerInterface, notificationType);
                }
            }
        }
    }
}

public class MediatorRegistrationOptions
{
    public List<Assembly> ServiceAssemblies = [];
}
