namespace ResiliencePatterns.Core;

/// <summary>
/// Defines a resilience strategy that wraps the execution of operations with fault-tolerance mechanisms.
/// </summary>
/// <typeparam name="T">The type of the result returned by the operation.</typeparam>
/// <remarks>
/// Implementations can provide various resilience patterns such as:
/// - Circuit Breaker: Prevent calls to failing services
/// - Retry: Automatically retry failed operations
/// - Timeout: Set maximum execution time
/// - Bulkhead: Isolate resources
/// </remarks>
public interface IResilienceStrategy<T>
{
    /// <summary>
    /// Executes the specified action with the resilience strategy applied.
    /// </summary>
    /// <param name="action">The async operation to execute with resilience handling.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation, containing the result of the action.</returns>
    /// <exception cref="CircuitBreakerOpenException">Thrown when the circuit breaker is open and requests are being blocked.</exception>
    Task<T> ExecuteAsync(Func<CancellationToken, Task<T>> action, CancellationToken cancellationToken = default);
}
