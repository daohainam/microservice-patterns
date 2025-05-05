namespace ResiliencePatterns.Core;
public interface IResilienceStrategy<T>
{
    Task<T> ExecuteAsync(Func<CancellationToken, Task<T>> action, CancellationToken cancellationToken = default);
}
