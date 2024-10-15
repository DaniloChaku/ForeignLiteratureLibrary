using DALTests.TestHelpers;
using FluentAssertions;
using ForeignLiteratureLibrary.DAL.Entities;
using ForeignLiteratureLibrary.DAL.Exceptions;
using ForeignLiteratureLibrary.DAL.Repositories;
using Microsoft.Data.SqlClient;

namespace DALTests.Repositories;

public class ReaderRepositoryTests : IDisposable
{
    private readonly ReaderRepository _repository;
    private readonly string _connectionString = TestConnectionStringHelper.ConnectionString;
    private bool _disposed;

    public ReaderRepositoryTests()
    {
        _repository = new ReaderRepository(_connectionString);
        DbSeedHelper.SeedDb(_connectionString);
    }

    [Fact]
    public async Task GetByLibraryCardNumberAsync_ExistingReader_ReturnsReaderWithLoans()
    {
        // Act
        var result = await _repository.GetByLibraryCardNumberAsync("1001");

        // Assert
        result.Should().NotBeNull();
        result!.LibraryCardNumber.Should().Be("1001");
        result.FullName.Should().Be("John Doe");
        result.Loans.Should().HaveCount(1);
        result.Loans.Should().Contain(l => l.BookEditionID == 1);
    }

    [Fact]
    public async Task GetByLibraryCardNumberAsync_NonExistingReader_ReturnsNull()
    {
        // Act
        var result = await _repository.GetByLibraryCardNumberAsync("9999");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByFullNameAsync_ExistingName_ReturnsMatchingReaders()
    {
        // Act
        var results = await _repository.GetByFullNameAsync("Jane");

        // Assert
        results.Should().HaveCount(2);
        results.Should().Contain(r => r.FullName == "Jane Smith");
        results.Should().Contain(r => r.FullName == "Jane Doe");
    }

    [Fact]
    public async Task GetPageAsync_ReturnsCorrectPage()
    {
        // Act
        var page1 = await _repository.GetPageAsync(1, 2);
        var page2 = await _repository.GetPageAsync(2, 2);

        // Assert
        page1.Should().HaveCount(2);
        page2.Should().HaveCount(1);
    }

    [Fact]
    public async Task AddAsync_AddsNewReader()
    {
        // Arrange
        var newReader = new Reader
        {
            LibraryCardNumber = "1004",
            FullName = "New Reader",
            DateOfBirth = new DateTime(1995, 1, 1),
            Email = "new.reader@example.com",
            Phone = "+1234567890",
            RegistrationDate = DateTime.Today
        };

        // Act
        await _repository.AddAsync(newReader);
        var result = await _repository.GetByLibraryCardNumberAsync("1004");

        // Assert
        result.Should().NotBeNull();
        result!.LibraryCardNumber.Should().Be("1004");
        result.FullName.Should().Be("New Reader");
    }

    [Fact]
    public async Task UpdateAsync_UpdatesExistingReader()
    {
        // Arrange
        var reader = await _repository.GetByLibraryCardNumberAsync("1001");
        reader!.Phone = "+999999999";

        // Act
        await _repository.UpdateAsync(reader);
        var updatedReader = await _repository.GetByLibraryCardNumberAsync("1001");

        // Assert
        updatedReader.Should().NotBeNull();
        updatedReader!.Phone.Should().Be("+999999999");
    }

    [Fact]
    public async Task DeleteAsync_DeletesExistingReader()
    {
        // Arrange
        var cardNumber = "1003";

        // Act
        await _repository.DeleteAsync(cardNumber);
        var deletedReader = await _repository.GetByLibraryCardNumberAsync(cardNumber);

        // Assert
        deletedReader.Should().BeNull();
    }

    [Fact]
    public async Task GetCountAsync_ReturnsCorrectCount()
    {
        // Act
        var count = await _repository.GetCountAsync();

        // Assert
        count.Should().Be(3);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public async Task AddAsync_EmptyFullName_ThrowsException(string invalidName)
    {
        // Arrange
        var invalidReader = new Reader
        {
            LibraryCardNumber = "1004",
            FullName = invalidName,
            DateOfBirth = new DateTime(1995, 1, 1),
            RegistrationDate = DateTime.Today
        };

        // Act & Assert
        await Assert.ThrowsAsync<CheckConstraintViolationException>(() => _repository.AddAsync(invalidReader));
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
