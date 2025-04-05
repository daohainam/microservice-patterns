namespace TransactionalOutbox.Infrastructure;
public class PollingOutboxMessageRepositoryOptions
{
    public int MaxRetries { get; set; } = 3;
}

