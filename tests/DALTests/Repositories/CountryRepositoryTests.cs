using DALTests.TestHelpers;
using FluentAssertions;
using ForeignLiteratureLibrary.DAL.Entities;
using ForeignLiteratureLibrary.DAL.Exceptions;
using ForeignLiteratureLibrary.DAL.Repositories;
using Microsoft.Data.SqlClient;

namespace DALTests.Repositories;

public class CountryRepositoryTests : IDisposable
{
    private readonly CountryRepository _repository;
    private readonly string _connectionString = TestConnectionStringHelper.ConnectionString;
    private bool _disposed = false;

    public CountryRepositoryTests()
    {
        _repository = new CountryRepository(_connectionString);
        DbSeedHelper.SeedDb(_connectionString);
    }

    [Fact]
    public async Task GetByCodeAsync_ExistingCode_ReturnsCountry()
    {
        // Act
        var result = await _repository.GetByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.CountryName.Should().Be("United States");
    }

    [Fact]
    public async Task GetByCodeAsync_NonExistingCode_ReturnsNull()
    {
        // Act
        var result = await _repository.GetByIdAsync(99);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllCountries()
    {
        // Act
        var results = await _repository.GetAllAsync();

        // Assert
        results.Should().HaveCount(4);
        results.Should().Contain(c => c.CountryName == "United States");
        results.Should().Contain(c => c.CountryName == "Germany");
        results.Should().Contain(c => c.CountryName == "France");
        results.Should().Contain(c => c.CountryName == "Ukraine");
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
    public async Task AddAsync_AddsNewCountry()
    {
        // Arrange
        var newCountry = new Country { CountryName = "Portugal" };

        // Act
        await _repository.AddAsync(newCountry);
        var result = await _repository.GetByIdAsync(5);

        // Assert
        result.Should().NotBeNull();
        result!.CountryName.Should().Be("Portugal");
    }

    [Fact]
    public async Task UpdateAsync_UpdatesExistingCountry()
    {
        // Arrange
        var id = 1;
        var country = await _repository.GetByIdAsync(id);
        country!.CountryName = "United States of America";

        // Act
        await _repository.UpdateAsync(country);
        var updatedCountry = await _repository.GetByIdAsync(id);

        // Assert
        updatedCountry.Should().NotBeNull();
        updatedCountry!.CountryName.Should().Be("United States of America");
    }

    [Fact]
    public async Task DeleteAsync_DeletesExistingCountry()
    {
        // Act
        var id = 4;
        await _repository.DeleteAsync(id);
        var deletedCountry = await _repository.GetByIdAsync(id);

        // Assert
        deletedCountry.Should().BeNull();
    }

    [Fact]
    public async Task AddAsync_DuplicateCountryName_ThrowsException()
    {
        // Arrange
        var duplicateCountry = new Country { CountryName = "Ukraine" };

        // Act & Assert
        await Assert.ThrowsAsync<UniqueConstraintViolationException>(() => _repository.AddAsync(duplicateCountry));
    }

    [Fact]
    public async Task AddAsync_NullCountryName_ThrowsException()
    {
        // Arrange
        var invalidCountryNullName = new Country { CountryName = null! };

        // Act & Assert
        await Assert.ThrowsAsync<NotNullConstraintViolationException>(() => _repository.AddAsync(invalidCountryNullName));
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
