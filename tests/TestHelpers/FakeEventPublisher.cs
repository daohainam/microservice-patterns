using EventBus.Abstractions;
using EventBus.Events;

namespace TestHelpers
{
    public class FakeEventPublisher : IEventPublisher
    {
        public List<IntegrationEvent> Events { get; } = [];

        public Task PublishAsync<TEvent>(TEvent @event) where TEvent : IntegrationEvent
        {
            Events.Add(@event);

            return Task.CompletedTask;
        }
    }
}
