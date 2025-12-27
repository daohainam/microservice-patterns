using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Mediator;
internal class Mediator(MediatorRegistrationContainer container, IServiceProvider serviceProvider, ILogger<Mediator> logger) : IMediator
{
    public async Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default) where TNotification : INotification
    {
        var handlers = container.FindHandlers(notification);
        if (handlers.Any())
        {
            foreach (var handlerType in handlers)
            {
                using var scope = serviceProvider.CreateScope();
                var handler = ActivatorUtilities.CreateInstance(scope.ServiceProvider, handlerType);

                if (handler is null)
                {
                    logger.LogWarning("Handler of type {HandlerType} could not be created for notification of type {NotificationType}.",
                        handlerType.Name, typeof(TNotification).Name);
                    continue;
                }

                var handleMethod = handlerType.GetMethod("Handle", [notification.GetType(), typeof(CancellationToken)]);

                if (handleMethod is not null)
                {
                    try
                    {
                        if (handleMethod.Invoke(handler, [notification, cancellationToken]) is Task task)
                        {
                            await task;
                        }
                        logger.LogInformation("Successfully handled notification of type {NotificationType} with handler {HandlerType}.",
                            typeof(TNotification).Name, handlerType.Name);

                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Error occurred while handling notification of type {NotificationType} with handler {HandlerType}.",
                            typeof(TNotification).Name, handlerType.Name);
                    }
                    finally
                    {
                        if (handler is IDisposable disposableHandler)
                        {
                            disposableHandler.Dispose();
                        }
                    }
                }
                else
                {
                    throw new InvalidOperationException($"Handler {handlerType.Name} does not implement INotificationHandler<{typeof(TNotification).Name}>.");
                }
            }
        }
        else
        {
            throw new InvalidOperationException($"No handlers found for notification type {typeof(TNotification).Name}.");
        }
    }

}

internal class MediatorHandlerRegistration(Type notificationType, Type handlerType)
{
    public Type HandlerType { get; } = handlerType;
    public Type NotificationType { get; } = notificationType;
    public override string ToString()
    {
        return $"{HandlerType.Name} handling notifications: {NotificationType}";
    }
}

internal class MediatorRegistrationContainer
{
    private readonly Dictionary<Type, List<MediatorHandlerRegistration>> handlers = [];

    internal IEnumerable<Type> FindHandlers<TNotification>(TNotification notification) where TNotification : INotification
    {
        var type = notification.GetType();
        if (handlers.TryGetValue(type, out var registrations))
        {
            return registrations.Select(r => r.HandlerType);
        }
        else
        {
            return [];
        }
    }

    internal void RegisterHandler(Type notificationType, Type handlerType)
    {
        if (!typeof(INotification).IsAssignableFrom(notificationType))
        {
            throw new ArgumentException($"Type {notificationType.Name} does not implement INotification interface.");
        }

        if (!handlers.TryGetValue(notificationType, out var registrations))
        {
            registrations = [];
            handlers[notificationType] = registrations;
        }

        registrations.Add(new MediatorHandlerRegistration(notificationType, handlerType));
    }

}
