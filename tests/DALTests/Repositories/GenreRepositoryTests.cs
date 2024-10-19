using DALTests.TestHelpers;
using FluentAssertions;
using ForeignLiteratureLibrary.DAL.Entities;
using ForeignLiteratureLibrary.DAL.Exceptions;
using ForeignLiteratureLibrary.DAL.Repositories;
using Microsoft.Data.SqlClient;

namespace DALTests.Repositories;

public class GenreRepositoryTests : IDisposable
{
    private readonly GenreRepository _repository;
    private readonly string _connectionString = TestConnectionStringHelper.ConnectionString;
    private bool _disposed;

    public GenreRepositoryTests()
    {
        _repository = new GenreRepository(_connectionString);
        DbSeedHelper.SeedDb(_connectionString);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsGenre()
    {
        // Arrange
        var id = 1;

        // Act
        var result = await _repository.GetByIdAsync(id);

        // Assert
        result.Should().NotBeNull();
        result!.GenreID.Should().Be(id);
        result.GenreName.Should().Be("Fiction");
        result.Books.Should().HaveCount(1);
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
    public async Task GetAllAsync_ReturnsAllGenres()
    {
        // Act
        var results = await _repository.GetAllAsync();

        // Assert
        results.Should().HaveCount(4);
        results.Select(g => g.GenreName).Should().BeEquivalentTo(
            "Fiction", "Philosophy", "Historical Novel", "Science Fiction");
    }

    [Fact]
    public async Task GetPageAsync_ReturnsCorrectPage()
    {
        // Act
        var page1 = await _repository.GetPageAsync(1, 2);
        var page2 = await _repository.GetPageAsync(2, 3);

        // Assert
        page1.Should().HaveCount(2);
        page2.Should().HaveCount(1);
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
    public async Task AddAsync_AddsNewGenre()
    {
        // Arrange
        var newGenre = new Genre { GenreName = "Romanance" };

        // Act
        await _repository.AddAsync(newGenre);

        // Assert
        newGenre.GenreID.Should().BeGreaterThan(0);
        var result = await _repository.GetByIdAsync(newGenre.GenreID);
        result.Should().NotBeNull();
        result!.GenreName.Should().Be("Romanance");
    }

    [Fact]
    public async Task UpdateAsync_UpdatesExistingGenre()
    {
        // Arrange
        var id = 1;
        var genreToUpdate = await _repository.GetByIdAsync(id);
        genreToUpdate!.GenreName = "Contemporary Fiction";

        // Act
        await _repository.UpdateAsync(genreToUpdate);
        var updatedGenre = await _repository.GetByIdAsync(id);

        // Assert
        updatedGenre.Should().NotBeNull();
        updatedGenre!.GenreName.Should().Be("Contemporary Fiction");
    }

    [Fact]
    public async Task DeleteAsync_DeletesExistingGenre()
    {
        // Arrange
        var id = 4;

        // Act
        await _repository.DeleteAsync(id);
        var deletedGenre = await _repository.GetByIdAsync(id);

        // Assert
        deletedGenre.Should().BeNull();
    }

    [Fact]
    public async Task AddAsync_DuplicateName_ThrowsException()
    {
        // Arrange
        var duplicateGenre = new Genre { GenreName = "Fiction" };

        // Act & Assert
        await Assert.ThrowsAsync<UniqueConstraintViolationException>(() => _repository.AddAsync(duplicateGenre));
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public async Task AddAsync_InvalidName_ThrowsException(string invalidName)
    {
        // Arrange
        var invalidGenre = new Genre { GenreName = invalidName };

        // Act & Assert
        await Assert.ThrowsAnyAsync<Exception>(() => _repository.AddAsync(invalidGenre));
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
