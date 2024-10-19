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
        result.AuthorFullName.Should().Be("J.D. Salinger");
        result.Country.Should().NotBeNull();
        result.Country!.CountryName.Should().NotBeNull("United States");
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
        var newAuthor = new Author 
        { 
            AuthorFullName = "Friedrich Nietzsche", 
            CountryID = 1,
            BirthYear = 1991,
            DeathYear = 2004
        };

        // Act
        await _repository.AddAsync(newAuthor);

        // Assert
        newAuthor.AuthorID.Should().BeGreaterThan(0);
        var result = await _repository.GetByIdAsync(newAuthor.AuthorID);
        result.Should().NotBeNull();
        result!.AuthorFullName.Should().Be("Friedrich Nietzsche");
    }

    [Fact]
    public async Task UpdateAsync_UpdatesExistingAuthor()
    {
        // Arrange
        var id = 1;
        var author = await _repository.GetByIdAsync(id);
        author!.AuthorFullName = "Jerome David Salinger";

        // Act
        await _repository.UpdateAsync(author);
        var updatedAuthor = await _repository.GetByIdAsync(id);

        // Assert
        updatedAuthor.Should().NotBeNull();
        updatedAuthor!.AuthorFullName.Should().Be("Jerome David Salinger");
    }

    [Fact]
    public async Task DeleteAsync_DeletesAuthorWithoutBooks()
    {
        // Act
        var id = 4;
        await _repository.DeleteAsync(id);
        var deletedAuthor = await _repository.GetByIdAsync(id);

        // Assert
        deletedAuthor.Should().BeNull();
    }

    [Fact]
    public async Task GetTop10AuthorsAsync_ShouldReturnTopAuthors()
    {
        // Arrange
        var startDate = new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var endDate = new DateTime(2023, 12, 31, 0, 0, 0, DateTimeKind.Utc);

        // Act
        var topAuthors = await _repository.GetTop10AuthorsAsync(startDate, endDate);

        // Assert
        topAuthors.Should().NotBeNull();
        topAuthors.Should().HaveCount(2);

        topAuthors.Should().ContainSingle(x => x.AuthorFullName == "J.D. Salinger").Which.LoanCount.Should().Be(1);
        topAuthors.Should().ContainSingle(x => x.AuthorFullName == "Victor Hugo").Which.LoanCount.Should().Be(1);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task AddAsync_InvalidFullName_ThrowsSqlException(string invalidName)
    {
        // Arrange
        var invalidAuthor = new Author { AuthorFullName = invalidName, CountryID = 1 };

        // Act & Assert
        await Assert.ThrowsAnyAsync<Exception>(() => _repository.AddAsync(invalidAuthor));
    }

    [Fact]
    public async Task AddAsync_NonExistentCountryID_ThrowsForeignKeyViolationException()
    {
        // Arrange
        var authorWithInvalidCountry = new Author { AuthorFullName = "Test Author", CountryID = 99 };

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
        authors.Should().Contain(a => a.AuthorFullName == "J.D. Salinger" && a.CountryID == 1);
        authors.Should().Contain(a => a.AuthorFullName == "Victor Hugo" && a.CountryID == 2);
        authors.Should().Contain(a => a.AuthorFullName == "Johann Wolfgang von Goethe" && a.CountryID == 3);
        authors.Should().Contain(a => a.AuthorFullName == "Klaus Mann" && a.CountryID == 3);
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
