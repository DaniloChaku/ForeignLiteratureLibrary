using DALTests.TestHelpers;
using FluentAssertions;
using ForeignLiteratureLibrary.DAL.Entities;
using ForeignLiteratureLibrary.DAL.Repositories;
using Microsoft.Data.SqlClient;

namespace DALTests.Repositories;

public class TranslatorRepositoryTests : IDisposable
{
    private readonly TranslatorRepository _repository;
    private readonly string _connectionString = TestConnectionStringHelper.ConnectionString;
    private bool _disposed;

    public TranslatorRepositoryTests()
    {
        _repository = new TranslatorRepository(_connectionString);
        DbSeedHelper.SeedDb(_connectionString);
    }

    [Fact]
    public async Task GetByIdWithBookEditionsAsync_ExistingId_ReturnsTranslatorWithBookEditions()
    {
        // Act
        var translator = await _repository.GetByIdWithBookEditionsAsync(1);

        // Assert
        translator.Should().NotBeNull();
        translator!.TranslatorID.Should().Be(1);
        translator.FullName.Should().Be("Charles Wilbour");
        translator.CountryCode.Should().Be("US");
        translator.Country.Should().NotBeNull();
        translator.Country!.Name.Should().Be("United States");
        translator.BookEditions.Should().HaveCount(1);
        translator.BookEditions.First().BookEditionID.Should().Be(2);
    }

    [Fact]
    public async Task GetByIdWithBookEditionsAsync_NonExistingId_ReturnsNull()
    {
        // Act
        var translator = await _repository.GetByIdWithBookEditionsAsync(999);

        // Assert
        translator.Should().BeNull();
    }

    [Fact]
    public async Task AddAsync_AddsNewTranslator()
    {
        // Arrange
        var newTranslator = new Translator
        {
            FullName = "John Doe",
            CountryCode = "US"
        };

        // Act
        await _repository.AddAsync(newTranslator);

        // Assert
        newTranslator.TranslatorID.Should().BeGreaterThan(0);
        var addedTranslator = await _repository.GetByIdWithBookEditionsAsync(newTranslator.TranslatorID);
        addedTranslator.Should().NotBeNull();
        addedTranslator!.FullName.Should().Be("John Doe");
        addedTranslator.CountryCode.Should().Be("US");
    }

    [Fact]
    public async Task UpdateAsync_UpdatesExistingTranslator()
    {
        // Arrange
        var translator = await _repository.GetByIdWithBookEditionsAsync(2);
        translator!.FullName = "Philip Wayne Updated";

        // Act
        await _repository.UpdateAsync(translator);

        // Assert
        var updatedTranslator = await _repository.GetByIdWithBookEditionsAsync(2);
        updatedTranslator.Should().NotBeNull();
        updatedTranslator!.FullName.Should().Be("Philip Wayne Updated");
    }

    [Fact]
    public async Task DeleteAsync_DeletesTranslatorAndAssociations()
    {
        // Act
        await _repository.DeleteAsync(4);
        var deletedTranslator = await _repository.GetByIdWithBookEditionsAsync(4);

        // Assert
        deletedTranslator.Should().BeNull();
    }

    [Fact]
    public async Task GetCountAsync_ReturnsCorrectCount()
    {
        // Act
        var count = await _repository.GetCountAsync();

        // Assert
        count.Should().Be(3);
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

        page1.TrueForAll(t => t.Country != null);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task AddAsync_InvalidName_ThrowsException(string invalidName)
    {
        // Arrange
        var invalidTranslator = new Translator { FullName = invalidName, CountryCode = "US" };

        // Act & Assert
        await Assert.ThrowsAsync<SqlException>(() => _repository.AddAsync(invalidTranslator));
    }

    [Fact]
    public async Task AddAsync_NonExistentCountryCode_ThrowsException()
    {
        // Arrange
        var invalidTranslator = new Translator { FullName = "John Doe", CountryCode = "XX" };

        // Act & Assert
        await Assert.ThrowsAsync<SqlException>(() => _repository.AddAsync(invalidTranslator));
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
