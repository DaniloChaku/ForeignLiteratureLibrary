using DALTests.TestHelpers;
using FluentAssertions;
using ForeignLiteratureLibrary.DAL.Entities;
using ForeignLiteratureLibrary.DAL.Exceptions;
using ForeignLiteratureLibrary.DAL.Repositories;
using Microsoft.Data.SqlClient;

namespace DALTests.Repositories;

public class AuthorRepositoryTests : IDisposable
{
    private readonly AuthorRepository _repository;
    private readonly string _connectionString = TestConnectionStringHelper.ConnectionString;
    private bool _disposed;

    public AuthorRepositoryTests()
    {
        _repository = new AuthorRepository(_connectionString);
        DbSeedHelper.SeedDb(_connectionString);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsAuthorWithBooks()
    {
        // Act
        var result = await _repository.GetByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.AuthorID.Should().Be(1);
        result.FullName.Should().Be("J.D. Salinger");
        result.CountryCode.Should().Be("US");
        result.Country.Should().NotBeNull();
        result.Books.Should().NotBeEmpty();
        result.Books.Should().Contain(b => b.OriginalTitle == "The Catcher in the Rye");
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
        page1.TrueForAll(a => a.Country != null);
    }

    [Fact]
    public async Task AddAsync_AddsNewAuthor()
    {
        // Arrange
        var newAuthor = new Author { FullName = "Friedrich Nietzsche", CountryCode = "DE" };

        // Act
        await _repository.AddAsync(newAuthor);

        // Assert
        newAuthor.AuthorID.Should().BeGreaterThan(0);
        var result = await _repository.GetByIdAsync(newAuthor.AuthorID);
        result.Should().NotBeNull();
        result!.FullName.Should().Be("Friedrich Nietzsche");
    }

    [Fact]
    public async Task UpdateAsync_UpdatesExistingAuthor()
    {
        // Arrange
        var author = await _repository.GetByIdAsync(1);
        author!.FullName = "Jerome David Salinger";

        // Act
        await _repository.UpdateAsync(author);
        var updatedAuthor = await _repository.GetByIdAsync(1);

        // Assert
        updatedAuthor.Should().NotBeNull();
        updatedAuthor!.FullName.Should().Be("Jerome David Salinger");
    }

    [Fact]
    public async Task DeleteAsync_DeletesAuthorWithoutBooks()
    {
        // Act
        await _repository.DeleteAsync(4);
        var deletedAuthor = await _repository.GetByIdAsync(4);

        // Assert
        deletedAuthor.Should().BeNull();
    }

    [Fact]
    public async Task GetTop10AuthorsAsync_ShouldReturnTopAuthors()
    {
        // Arrange
        var startDate = new DateTime(2023, 1, 1);
        var endDate = new DateTime(2023, 12, 31);

        // Act
        var topAuthors = await _repository.GetTop10AuthorsAsync(startDate, endDate);

        // Assert
        topAuthors.Should().NotBeNull();
        topAuthors.Should().HaveCount(2);

        topAuthors.Should().ContainSingle(x => x.FullName == "J.D. Salinger").Which.LoanCount.Should().Be(1);
        topAuthors.Should().ContainSingle(x => x.FullName == "Victor Hugo").Which.LoanCount.Should().Be(1);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task AddAsync_InvalidFullName_ThrowsSqlException(string invalidName)
    {
        // Arrange
        var invalidAuthor = new Author { FullName = invalidName, CountryCode = "US" };

        // Act & Assert
        await Assert.ThrowsAnyAsync<Exception>(() => _repository.AddAsync(invalidAuthor));
    }

    [Fact]
    public async Task AddAsync_NonExistentCountryCode_ThrowsSqlException()
    {
        // Arrange
        var authorWithInvalidCountry = new Author { FullName = "Test Author", CountryCode = "XX" };

        // Act & Assert
        await Assert.ThrowsAnyAsync<ForeignKeyViolationException>(() => _repository.AddAsync(authorWithInvalidCountry));
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllAuthors()
    {
        // Act
        var authors = await _repository.GetAllAsync();

        // Assert
        authors.Should().HaveCount(4);
        authors.Should().Contain(a => a.FullName == "J.D. Salinger" && a.CountryCode == "US");
        authors.Should().Contain(a => a.FullName == "Victor Hugo" && a.CountryCode == "FR");
        authors.Should().Contain(a => a.FullName == "Johann Wolfgang von Goethe" && a.CountryCode == "DE");
        authors.Should().Contain(a => a.FullName == "Klaus Mann" && a.CountryCode == "DE");
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
