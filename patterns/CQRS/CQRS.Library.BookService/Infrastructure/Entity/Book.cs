namespace CQRS.Library.BookService.Infrastructure.Entity
{
    public class Book
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = default!;
        public string Author { get; set; } = default!;
    }
}
