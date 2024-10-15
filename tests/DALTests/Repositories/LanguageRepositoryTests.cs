using DALTests.TestHelpers;
using FluentAssertions;
using ForeignLiteratureLibrary.DAL.Entities;
using ForeignLiteratureLibrary.DAL.Exceptions;
using ForeignLiteratureLibrary.DAL.Repositories;
using Microsoft.Data.SqlClient;

namespace DALTests.Repositories;

public class LanguageRepositoryTests : IDisposable
{
    private readonly LanguageRepository _repository;
    private readonly string _connectionString = TestConnectionStringHelper.ConnectionString;
    private bool _disposed = false;

    public LanguageRepositoryTests()
    {
        _repository = new LanguageRepository(_connectionString);
        DbSeedHelper.SeedDb(_connectionString);
    }

    [Fact]
    public async Task GetByCodeAsync_ExistingCode_ReturnsLanguage()
    {
        // Arrange
        const string languageCode = "EN";

        // Act
        var result = await _repository.GetByCodeAsync(languageCode);

        // Assert
        result.Should().NotBeNull();
        result!.LanguageCode.Should().Be(languageCode);
        result.Name.Should().Be("English");
    }

    [Fact]
    public async Task GetByCodeAsync_NonExistingCode_ReturnsNull()
    {
        // Arrange
        const string languageCode = "XX";

        // Act
        var result = await _repository.GetByCodeAsync(languageCode);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllLanguages()
    {
        // Act
        var results = await _repository.GetAllAsync();

        // Assert
        results.Should().HaveCount(4);
        results.Should().Contain(l => l.LanguageCode == "EN" && l.Name == "English");
        results.Should().Contain(l => l.LanguageCode == "FR" && l.Name == "French");
        results.Should().Contain(l => l.LanguageCode == "DE" && l.Name == "German");
        results.Should().Contain(l => l.LanguageCode == "UA" && l.Name == "Ukrainian");
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
    public async Task AddAsync_AddsNewLanguage()
    {
        // Arrange
        var newLanguage = new Language { LanguageCode = "ES", Name = "Spanish" };

        // Act
        await _repository.AddAsync(newLanguage);
        var result = await _repository.GetByCodeAsync("ES");

        // Assert
        result.Should().NotBeNull();
        result!.LanguageCode.Should().Be("ES");
        result.Name.Should().Be("Spanish");
    }

    [Fact]
    public async Task UpdateAsync_UpdatesExistingLanguage()
    {
        // Arrange
        var language = await _repository.GetByCodeAsync("EN");
        language!.Name = "British English";

        // Act
        await _repository.UpdateAsync(language);
        var updatedLanguage = await _repository.GetByCodeAsync("EN");

        // Assert
        updatedLanguage.Should().NotBeNull();
        updatedLanguage!.Name.Should().Be("British English");
    }

    [Fact]
    public async Task DeleteAsync_DeletesExistingLanguage()
    {
        // Act
        await _repository.DeleteAsync("UA");
        var deletedLanguage = await _repository.GetByCodeAsync("UA");

        // Assert
        deletedLanguage.Should().BeNull();
    }

    [Fact]
    public async Task AddAsync_DuplicateLanguageCode_ThrowsException()
    {
        // Arrange
        var duplicateLanguage = new Language { LanguageCode = "EN", Name = "English (US)" };

        // Act & Assert
        await Assert.ThrowsAsync<UniqueConstraintViolationException>(() => _repository.AddAsync(duplicateLanguage));
    }

    [Theory]
    [InlineData("  ")]
    [InlineData(null)]
    public async Task AddAsync_InvalidLanguageCode_ThrowsException(string invalidLanguageCode)
    {
        // Arrange
        var invalidLanguage = new Language { LanguageCode = invalidLanguageCode, Name = "English (US)" };

        // Act & Assert
        await Assert.ThrowsAnyAsync<Exception>(() => _repository.AddAsync(invalidLanguage));
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
