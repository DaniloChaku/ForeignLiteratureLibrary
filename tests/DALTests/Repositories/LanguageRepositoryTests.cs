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
    public async Task GetByIdAsync_ExistingCode_ReturnsLanguage()
    {
        // Arrange
        int languageId = 1;

        // Act
        var result = await _repository.GetByIdAsync(languageId);

        // Assert
        result.Should().NotBeNull();
        result!.LanguageID.Should().Be(languageId);
        result.LanguageName.Should().Be("English");
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingCode_ReturnsNull()
    {
        // Arrange
        int languageId = 99;

        // Act
        var result = await _repository.GetByIdAsync(languageId);

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
        results.Should().Contain(l => l.LanguageID == 1 && l.LanguageName == "English");
        results.Should().Contain(l => l.LanguageID == 2 && l.LanguageName == "French");
        results.Should().Contain(l => l.LanguageID == 3 && l.LanguageName == "German");
        results.Should().Contain(l => l.LanguageID == 4 && l.LanguageName == "Ukrainian");
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
        var newLanguage = new Language { LanguageName = "Spanish" };

        // Act
        await _repository.AddAsync(newLanguage);
        var result = await _repository.GetByIdAsync(5);

        // Assert
        result.Should().NotBeNull();
        result!.LanguageName.Should().Be("Spanish");
    }

    [Fact]
    public async Task UpdateAsync_UpdatesExistingLanguage()
    {
        // Arrange
        var id = 1;
        var language = await _repository.GetByIdAsync(id);
        language!.LanguageName = "British English";

        // Act
        await _repository.UpdateAsync(language);
        var updatedLanguage = await _repository.GetByIdAsync(id);

        // Assert
        updatedLanguage.Should().NotBeNull();
        updatedLanguage!.LanguageName.Should().Be("British English");
    }

    [Fact]
    public async Task DeleteAsync_DeletesExistingLanguage()
    {
        // Act
        var id = 4;
        await _repository.DeleteAsync(id);
        var deletedLanguage = await _repository.GetByIdAsync(id);

        // Assert
        deletedLanguage.Should().BeNull();
    }

    [Fact]
    public async Task AddAsync_DuplicateLanguageName_ThrowsException()
    {
        // Arrange
        var duplicateLanguage = new Language { LanguageName = "English" };

        // Act & Assert
        await Assert.ThrowsAsync<UniqueConstraintViolationException>(() => _repository.AddAsync(duplicateLanguage));
    }

    [Theory]
    [InlineData("  ")]
    [InlineData(null)]
    public async Task AddAsync_InvalidLanguageName_ThrowsException(string invalidLanguageName)
    {
        // Arrange
        var invalidLanguage = new Language { LanguageName = invalidLanguageName};

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
