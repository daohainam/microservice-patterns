using Aspire.Hosting;
using CQRS.Library.BorrowingHistoryService.Infrastructure.Entity;
using CQRS.Library.BorrowingService.Infrastructure.Entity;
using MicroservicePatterns.Shared.Pagination;
using System.Net.Http.Json;
using Book = CQRS.Library.BookService.Infrastructure.Entity.Book;
using Borrower = CQRS.Library.BorrowerService.Infrastructure.Entity.Borrower;

namespace IntegrationTests.Tests
{
    public class CQRSIntegrationTest : IClassFixture<AppFixture>
    {
        private readonly AppFixture fixture;
        private DistributedApplication App => fixture.App;

        public CQRSIntegrationTest(AppFixture fixture)
        {
            this.fixture = fixture;
        }

        [Fact]
        public async Task Create_Book_And_Read_Success()
        {
            // Arrange
            var resourceNotificationService = App.Services.GetRequiredService<ResourceNotificationService>();
            await resourceNotificationService.WaitForResourceAsync<Projects.CQRS_Library_BookService>(KnownResourceStates.Running).WaitAsync(TimeSpan.FromSeconds(30));

            // Act
            var httpClient = App.CreateHttpClient<Projects.CQRS_Library_BookService>();
            var bookId = Guid.NewGuid();
            var response = await httpClient.PostAsJsonAsync("/api/cqrs/v1/books", new Book()
            {
                Id = bookId,
                Title = "Test Book",
                Author = "Test Author"
            });

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Act
            response = await httpClient.GetAsync($"/api/cqrs/v1/books/{bookId}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var book = await response.Content.ReadFromJsonAsync<Book>();
            Assert.NotNull(book);
            Assert.Equal("Test Book", book.Title);
            Assert.Equal("Test Author", book.Author);
        }


        [Fact]
        public async Task Borrow_Book_And_Read_BorrowingHistory_Success()
        {
            // Arrange
            var resourceNotificationService = App.Services.GetRequiredService<ResourceNotificationService>();
            await resourceNotificationService.WaitForResourceAsync<Projects.CQRS_Library_BookService>(KnownResourceStates.Running).WaitAsync(TimeSpan.FromSeconds(30));

            // Act
            var bookHttpClient = App.CreateHttpClient<Projects.CQRS_Library_BookService>();
            var book = new Book()
            {
                Id = Guid.NewGuid(),
                Title = "Test Book",
                Author = "Test Author"
            };
            await bookHttpClient.PostAsJsonAsync("/api/cqrs/v1/books", book);

            var borrowerHttpClient = App.CreateHttpClient<Projects.CQRS_Library_BorrowerService>();
            var borrower = new Borrower()
            {
                Id = Guid.NewGuid(),
                Name = "Test Borrower",
                Address = "Test Address",
                Email = "test@email.com",
                PhoneNumber = "1234567890"
            };
            await borrowerHttpClient.PostAsJsonAsync("/api/cqrs/v1/borrowers", borrower);

            var borrowingHttpClient = App.CreateHttpClient<Projects.CQRS_Library_BorrowingService>();
            var borrowing = new Borrowing()
            {
                BookId = book.Id,
                BorrowerId = borrower.Id,
                BorrowedAt = DateTime.UtcNow,
                ValidUntil = DateTime.UtcNow.AddDays(7)
            };
            await borrowingHttpClient.PostAsJsonAsync("/api/cqrs/v1/borrowings", borrowing);

            await Task.Delay(4000);

            var borrowingHistoryHttpClient = App.CreateHttpClient<Projects.CQRS_Library_BorrowingHistoryService>();
            var response = await borrowingHistoryHttpClient.GetAsync($"/api/cqrs/v1/history/items?borrowerId={borrower.Id}&bookId={book.Id}");
            var borrowingHistoryItems = await response.Content.ReadFromJsonAsync<PaginatedResult<BorrowingHistoryItem>>();


            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(borrowingHistoryItems);
            Assert.Single(borrowingHistoryItems.Items);
            
            Assert.Equal(book.Id, borrowingHistoryItems.Items.First().BookId);
            Assert.Equal(book.Author, borrowingHistoryItems.Items.First().BookAuthor);
            Assert.Equal(book.Title, borrowingHistoryItems.Items.First().BookTitle);

            Assert.Equal(borrower.Id, borrowingHistoryItems.Items.First().BorrowerId);
            Assert.Equal(borrower.Name, borrowingHistoryItems.Items.First().BorrowerName);
            Assert.Equal(borrower.Address, borrowingHistoryItems.Items.First().BorrowerAddress);
            Assert.Equal(borrower.Email, borrowingHistoryItems.Items.First().BorrowerEmail);
            Assert.Equal(borrower.PhoneNumber, borrowingHistoryItems.Items.First().BorrowerPhoneNumber);
        }
    }
}
