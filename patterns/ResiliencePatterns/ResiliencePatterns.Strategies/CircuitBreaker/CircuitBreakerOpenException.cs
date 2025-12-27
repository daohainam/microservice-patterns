
namespace ResiliencePatterns.Strategies.CircuitBreaker;

/// <summary>
/// Exception thrown when an operation is attempted while the circuit breaker is in the Open state.
/// </summary>
[Serializable]
public class CircuitBreakerOpenException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CircuitBreakerOpenException"/> class.
    /// </summary>
    public CircuitBreakerOpenException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CircuitBreakerOpenException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public CircuitBreakerOpenException(string? message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CircuitBreakerOpenException"/> class with a specified error message 
    /// and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public CircuitBreakerOpenException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}