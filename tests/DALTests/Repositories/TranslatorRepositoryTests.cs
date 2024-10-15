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
    public async Task GetByIdAsync_ExistingId_ReturnsTranslatorWithBookEditions()
    {
        // Act
        var translator = await _repository.GetByIdAsync(1);

        // Assert
        translator.Should().NotBeNull();
        translator!.TranslatorID.Should().Be(1);
        translator.FullName.Should().Be("Charles Wilbour");
        translator.BookEditions.Should().HaveCount(1);
        translator.BookEditions.First().BookEditionID.Should().Be(2);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingId_ReturnsNull()
    {
        // Act
        var translator = await _repository.GetByIdAsync(999);

        // Assert
        translator.Should().BeNull();
    }

    [Fact]
    public async Task AddAsync_AddsNewTranslator()
    {
        // Arrange
        var newTranslator = new Translator
        {
            FullName = "John Doe"
        };

        // Act
        await _repository.AddAsync(newTranslator);

        // Assert
        newTranslator.TranslatorID.Should().BeGreaterThan(0);
        var addedTranslator = await _repository.GetByIdAsync(newTranslator.TranslatorID);
        addedTranslator.Should().NotBeNull();
        addedTranslator!.FullName.Should().Be("John Doe");
    }

    [Fact]
    public async Task UpdateAsync_UpdatesExistingTranslator()
    {
        // Arrange
        var translator = await _repository.GetByIdAsync(2);
        translator!.FullName = "Philip Wayne Updated";

        // Act
        await _repository.UpdateAsync(translator);

        // Assert
        var updatedTranslator = await _repository.GetByIdAsync(2);
        updatedTranslator.Should().NotBeNull();
        updatedTranslator!.FullName.Should().Be("Philip Wayne Updated");
    }

    [Fact]
    public async Task DeleteAsync_DeletesTranslatorAndAssociations()
    {
        // Act
        await _repository.DeleteAsync(4);
        var deletedTranslator = await _repository.GetByIdAsync(4);

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
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task AddAsync_InvalidName_ThrowsException(string invalidName)
    {
        // Arrange
        var invalidTranslator = new Translator { FullName = invalidName };

        // Act & Assert
        await Assert.ThrowsAnyAsync<Exception>(() => _repository.AddAsync(invalidTranslator));
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllTranslators()
    {
        // Act
        var translators = await _repository.GetAllAsync();

        // Assert
        translators.Should().HaveCount(3);
        translators.Should().Contain(t => t.FullName == "Charles Wilbour");
        translators.Should().Contain(t => t.FullName == "Philip Wayne");
        translators.Should().Contain(t => t.FullName == "Charles Baudelaire");
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
