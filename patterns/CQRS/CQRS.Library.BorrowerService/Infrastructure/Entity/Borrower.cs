namespace CQRS.Library.BorrowerService.Infrastructure.Entity;
public class Borrower
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string Address { get; set; } = default!;
    public string PhoneNumber { get; set; } = default!;
    public string Email { get; set; } = default!;
}
