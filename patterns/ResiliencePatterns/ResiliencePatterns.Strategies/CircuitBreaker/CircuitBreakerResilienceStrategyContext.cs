using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResiliencePatterns.Strategies.CircuitBreaker;
internal class CircuitBreakerResilienceStrategyContext(
    TimeSpan durationOfBreak,
    TimeSpan durationOfHalfOpen,
    int failureThreshold,
    int successThreshold)
{
    public CircuitBreakerResilienceStrategyContext(): this(
        TimeSpan.FromSeconds(30),
        TimeSpan.FromSeconds(10),
        5,
        2)
    {
    }
    public TimeSpan DurationOfBreak { get; } = durationOfBreak;
    public TimeSpan DurationOfHalfOpen { get; } = durationOfHalfOpen;
    public int FailureThreshold { get; } = failureThreshold;
    public int SuccessThreshold { get; } = successThreshold;
    public CircuitBreakerStates State { get; set; } = CircuitBreakerStates.Closed;
    public DateTime LastFailureTime { get; set; } = DateTime.MinValue;
    public DateTime LastSuccessTime { get; set; } = DateTime.MinValue;
    public int FailureCount { get; set; } = 0;
    public int SuccessCount { get; set; } = 0;

}

internal enum CircuitBreakerStates
{
    Closed,
    Open,
    HalfOpen
}
