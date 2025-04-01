namespace TransactionalOutbox.Infrastructure;
public class OutboxMessageRepositoryOptions
{
    public int MaxRetries { get; set; } = 3;
}
