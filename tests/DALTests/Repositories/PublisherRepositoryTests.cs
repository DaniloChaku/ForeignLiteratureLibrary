using DALTests.TestHelpers;
using FluentAssertions;
using ForeignLiteratureLibrary.DAL.Entities;
using ForeignLiteratureLibrary.DAL.Repositories;
using Microsoft.Data.SqlClient;

namespace DALTests.Repositories;

public class PublisherRepositoryTests : IDisposable
{
    private readonly PublisherRepository _repository;
    private readonly string _connectionString = TestConnectionStringHelper.ConnectionString;
    private bool _disposed = false;

    public PublisherRepositoryTests()
    {
        _repository = new PublisherRepository(_connectionString);
        DbSeedHelper.SeedDb(_connectionString);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsPublisher()
    {
        // Act
        var result = await _repository.GetByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.PublisherID.Should().Be(1);
        result.PublisherName.Should().Be("Penguin Books");
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
    public async Task GetAllAsync_ReturnsAllPublishers()
    {
        // Act
        var results = await _repository.GetAllAsync();

        // Assert
        results.Should().HaveCount(4);
        results.Select(p => p.PublisherName).Should().BeEquivalentTo(
            "Penguin Books", "Hachette Livre", "HarperCollins", "Simon and Schuster");
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
    public async Task AddAsync_AddsNewPublisher()
    {
        // Arrange
        var newPublisher = new Publisher { PublisherName = "Simon & Schuster", CountryID = 1 };

        // Act
        await _repository.AddAsync(newPublisher);

        // Assert
        newPublisher.PublisherID.Should().BeGreaterThan(0);
        var result = await _repository.GetByIdAsync(newPublisher.PublisherID);
        result.Should().NotBeNull();
        result!.PublisherName.Should().Be("Simon & Schuster");
    }

    [Fact]
    public async Task UpdateAsync_UpdatesExistingPublisher()
    {
        // Arrange
        var publishers = await _repository.GetAllAsync();
        var publisherToUpdate = publishers[0];
        var newName = "Penguin Random House";
        publisherToUpdate.PublisherName = newName;

        // Act
        await _repository.UpdateAsync(publisherToUpdate);
        var updatedPublisher = await _repository.GetByIdAsync(publisherToUpdate.PublisherID);

        // Assert
        updatedPublisher.Should().NotBeNull();
        updatedPublisher!.PublisherName.Should().Be(newName);
    }

    [Fact]
    public async Task DeleteAsync_DeletesExistingPublisher()
    {
        // Arrange
        var publisherId = 4;

        // Act
        await _repository.DeleteAsync(publisherId);
        var deletedPublisher = await _repository.GetByIdAsync(publisherId);

        // Assert
        deletedPublisher.Should().BeNull();
    }

    [Fact]
    public async Task AddAsync_DuplicateName_Allowed()
    {
        // Arrange
        var duplicatePublisher = new Publisher { PublisherName = "Penguin Books", CountryID = 1 };

        // Act
        Func<Task> act = async () => await _repository.AddAsync(duplicatePublisher);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public async Task AddAsync_InvalidName_ThrowsException(string invalidName)
    {
        // Arrange
        var invalidPublisher = new Publisher { PublisherName = invalidName };

        // Act & Assert
        await Assert.ThrowsAnyAsync<Exception>(() => _repository.AddAsync(invalidPublisher));
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
