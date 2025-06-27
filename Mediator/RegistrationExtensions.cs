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

        var container = services.RegisterHandlers(options.ServiceAssemblies);
        services.AddSingleton(container);
        services.AddSingleton<IMediator, Mediator>();

        return services;
    }

    private static MediatorRegistrationContainer RegisterHandlers(this IServiceCollection services, IEnumerable<Assembly> serviceAssemblies)
    {
        var container = new MediatorRegistrationContainer();
        var assemblies = new List<Assembly>() { Assembly.GetExecutingAssembly() };
        if (serviceAssemblies != null)
        {
            assemblies.AddRange(serviceAssemblies);
        }

        foreach (var assembly in assemblies)
        {
            var notificationHandlerTypes = assembly
                .GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && t.GetInterfaces().Any(i =>
                    i.IsGenericType && i.Name == "INotificationHandler`1"));

            if (notificationHandlerTypes.Any())
            {
                foreach (var notificationHandlerType in notificationHandlerTypes)
                {
                    var handlerInterfaces = notificationHandlerType
                        .GetInterfaces()
                        .Where(i => i.IsGenericType && i.GetGenericTypeDefinition().Name == "INotificationHandler`1");

                    foreach (var handlerInterface in handlerInterfaces)
                    {
                        if (handlerInterface.GenericTypeArguments.Length != 1)
                            continue;

                        var notificationType = handlerInterface.GenericTypeArguments[0];
                        container.RegisterHandler(notificationType, notificationHandlerType);
                    }
                }
            }
        }

        return container;
    }
}

public class MediatorRegistrationOptions
{
    public List<Assembly> ServiceAssemblies = [];
}
