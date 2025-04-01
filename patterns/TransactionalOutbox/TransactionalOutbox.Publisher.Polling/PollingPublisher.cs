using EventBus.Abstractions;
using TransactionalOutbox.Abstractions;

namespace TransactionalOutbox.Publisher.Polling;
public class PollingPublisher
{
    public PollingPublisher(IEventPublisher eventPublisher, IOutboxMessageRepository outboxMessageRepository)
    {
        EventPublisher = eventPublisher;
        OutboxMessageRepository = outboxMessageRepository;
    }

    public IEventPublisher EventPublisher { get; }
    public IOutboxMessageRepository OutboxMessageRepository { get; }
}
