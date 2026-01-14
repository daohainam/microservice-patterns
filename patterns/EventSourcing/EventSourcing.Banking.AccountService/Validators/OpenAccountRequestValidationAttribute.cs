namespace EventSourcing.Banking.AccountService.Validators;

[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
public class OpenAccountRequestValidationAttribute: ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not OpenAccountRequest request)
        {
            return new ValidationResult("Invalid request type");
        }
        if (string.IsNullOrWhiteSpace(request.AccountNumber))
        {
            return new ValidationResult("Account number is required");
        }
        if (string.IsNullOrWhiteSpace(request.Currency))
        {
            return new ValidationResult("Currency is required");
        }
        if (request.Balance < 0)
        {
            return new ValidationResult("Initial balance cannot be negative");
        }
        if (request.CreditLimit < 0)
        {
            return new ValidationResult("Credit limit cannot be negative");
        }
        return ValidationResult.Success;
    }
}
