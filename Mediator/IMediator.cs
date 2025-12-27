namespace Mediator;

/// <summary>
/// Defines a mediator to encapsulate request/response and publishing interaction patterns.
/// </summary>
public interface IMediator
{
    /// <summary>
    /// Asynchronously publishes a notification to multiple handlers.
    /// </summary>
    /// <typeparam name="TNotification">The type of notification being published.</typeparam>
    /// <param name="notification">The notification to publish.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous publish operation.</returns>
    Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
        where TNotification : INotification;
}
