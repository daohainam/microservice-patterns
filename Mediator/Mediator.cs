
namespace Mediator;
internal class Mediator(IServiceProvider serviceProvider) : IMediator
{
    public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default) where TNotification : INotification
    {
        throw new NotImplementedException();
    }
}

