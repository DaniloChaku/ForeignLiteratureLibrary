using DALTests.TestHelpers;
using FluentAssertions;
using ForeignLiteratureLibrary.DAL.Entities;
using ForeignLiteratureLibrary.DAL.Repositories;
using Microsoft.Data.SqlClient;

namespace DALTests.Repositories;

public class BookRepositoryTests : IDisposable
{
    private readonly BookRepository _repository;
    private readonly string _connectionString = TestConnectionStringHelper.ConnectionString;
    private bool _disposed;

    public BookRepositoryTests()
    {
        _repository = new BookRepository(_connectionString);
        DbSeedHelper.SeedDb(_connectionString);
    }

    [Fact]
    public async Task AddAsync_AddsNewBookWithAuthorsAndGenres()
    {
        // Arrange
        var newBook = new Book
        {
            OriginalTitle = "Thus Spoke Zarathustra",
            OriginalLanguageID = 1,
            FirstPublicationYear = 1883,
            Authors =
            [
                new() { AuthorID = 3 }
            ],
            Genres =
            [
                new() { GenreID = 2 }
            ]
        };

        // Act
        await _repository.AddAsync(newBook);

        // Assert
        newBook.BookID.Should().BeGreaterThan(0);
        var result = await _repository.GetByIdAsync(newBook.BookID);
        result.Should().NotBeNull();
        result!.OriginalTitle.Should().Be("Thus Spoke Zarathustra");
        result.Authors.Should().Contain(a => a.AuthorID == 3);
        result.Genres.Should().Contain(g => g.GenreID == 2);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public async Task AddAsync_InvalidTitle_ThrowsSqlException(string invalidTitle)
    {
        // Arrange
        var invalidBook = new Book
        {
            OriginalTitle = invalidTitle,
            OriginalLanguageID = 1,
            FirstPublicationYear = 1883,
        };

        // Act & Assert
        await Assert.ThrowsAnyAsync<Exception>(() => _repository.AddAsync(invalidBook));
    }

    [Fact]
    public async Task UpdateAsync_UpdatesExistingBook()
    {
        // Arrange
        var book = await _repository.GetByIdAsync(1); // Assuming Book with ID 1 exists
        book!.OriginalTitle = "The Catcher in the Rye - Updated";

        // Act
        await _repository.UpdateAsync(book);
        var updatedBook = await _repository.GetByIdAsync(1);

        // Assert
        updatedBook.Should().NotBeNull();
        updatedBook!.OriginalTitle.Should().Be("The Catcher in the Rye - Updated");
    }

    [Fact]
    public async Task DeleteAsync_DeletesBook()
    {
        // Arrange
        var id = 4;

        // Act
        await _repository.DeleteAsync(id);
        var deletedBook = await _repository.GetByIdAsync(4);

        // Assert
        deletedBook.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsBookWithDetails()
    {
        // Act
        var book = await _repository.GetByIdAsync(1);

        // Assert
        book.Should().NotBeNull();
        book!.OriginalTitle.Should().Be("The Catcher in the Rye");
        book.OriginalLanguage!.LanguageID.Should().Be(1);
        book.Authors.Should().NotBeEmpty();
        book.Genres.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingId_ReturnsNull()
    {
        // Act
        var result = await _repository.GetByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetCountAsync_ReturnsCorrectCount()
    {
        // Act
        var count = await _repository.GetCountAsync();

        // Assert
        count.Should().Be(4);
    }

    [Fact]
    public async Task GetPageAsync_ReturnsCorrectPage()
    {
        // Act
        var page1 = await _repository.GetPageAsync(1, 2);
        var page2 = await _repository.GetPageAsync(2, 2);

        // Assert
        page1.Should().HaveCount(2);
        page2.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetTop10BooksAsync_ReturnsTopBooks()
    {
        // Arrange
        var startDate = new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var endDate = new DateTime(2023, 12, 31, 0, 0, 0, DateTimeKind.Utc);

        // Act
        var topBooks = await _repository.GetTop10BooksAsync(startDate, endDate);

        // Assert
        topBooks.Should().NotBeNull();
        topBooks.Should().HaveCount(2);

        topBooks.Should().ContainSingle(x => x.OriginalTitle == "The Catcher in the Rye").Which.LoanCount.Should().Be(1);
        topBooks.Should().ContainSingle(x => x.OriginalTitle == "Les Misérables").Which.LoanCount.Should().Be(1);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            DbSeedHelper.DropTables(_connectionString);
        }

        _disposed = true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
