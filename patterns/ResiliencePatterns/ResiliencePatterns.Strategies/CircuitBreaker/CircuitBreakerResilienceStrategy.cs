using ResiliencePatterns.Core;

namespace ResiliencePatterns.Strategies.CircuitBreaker;
internal class CircuitBreakerResilienceStrategy<T>(CircuitBreakerResilienceStrategyContext context) : IResilienceStrategy<T>
{
    private readonly CircuitBreakerResilienceStrategyContext _context = context;

    public Task<T> ExecuteAsync(Func<CancellationToken, Task<T>> action, CancellationToken cancellationToken = default)
    {
        if (_context.State == CircuitBreakerStates.Open)
        {
            if (DateTime.UtcNow - _context.LastFailureTime < _context.DurationOfBreak)
            {
                // We do not allow execution if the circuit is open and the break duration has not passed
                throw new CircuitBreakerOpenException("Circuit breaker is open");
            }
            else
            {
                // If the break duration has passed, we move to half-open state
                _context.State = CircuitBreakerStates.HalfOpen;
            }
        }

        // Execute the action
        try
        {
            var result = action(cancellationToken);
            _context.SuccessCount++;
            _context.LastSuccessTime = DateTime.UtcNow;

            if (_context.State == CircuitBreakerStates.HalfOpen && _context.SuccessCount >= _context.SuccessThreshold)
            {
                // If the success threshold is reached, we close the circuit
                _context.State = CircuitBreakerStates.Closed;
                _context.SuccessCount = 0;
            }
            return result;
        }
        catch (Exception ex)
        {
            _context.FailureCount++;
            _context.LastFailureTime = DateTime.UtcNow;

            if (_context.State == CircuitBreakerStates.Closed && _context.FailureCount >= _context.FailureThreshold)
            {
                // If the failure threshold is reached, we open the circuit
                _context.State = CircuitBreakerStates.Open;
                _context.FailureCount = 0;
                throw new CircuitBreakerOpenException("Circuit breaker is open", ex);
            }
            throw;
        }

    }
}
