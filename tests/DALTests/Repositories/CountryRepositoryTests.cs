using DALTests.TestHelpers;
using FluentAssertions;
using ForeignLiteratureLibrary.DAL.Entities;
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
        var result = await _repository.GetByCodeAsync("US");

        // Assert
        result.Should().NotBeNull();
        result!.CountryCode.Should().Be("US");
        result.Name.Should().Be("United States");
    }

    [Fact]
    public async Task GetByCodeAsync_NonExistingCode_ReturnsNull()
    {
        // Act
        var result = await _repository.GetByCodeAsync("XX");

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
        results.Should().Contain(c => c.CountryCode == "US" && c.Name == "United States");
        results.Should().Contain(c => c.CountryCode == "DE" && c.Name == "Germany");
        results.Should().Contain(c => c.CountryCode == "FR" && c.Name == "France");
        results.Should().Contain(c => c.CountryCode == "UA" && c.Name == "Ukraine");
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
        var newCountry = new Country { CountryCode = "PT", Name = "Portugal" };

        // Act
        await _repository.AddAsync(newCountry);
        var result = await _repository.GetByCodeAsync("PT");

        // Assert
        result.Should().NotBeNull();
        result!.CountryCode.Should().Be("PT");
        result.Name.Should().Be("Portugal");
    }

    [Fact]
    public async Task UpdateAsync_UpdatesExistingCountry()
    {
        // Arrange
        var country = await _repository.GetByCodeAsync("US");
        country!.Name = "United States of America";

        // Act
        await _repository.UpdateAsync(country);
        var updatedCountry = await _repository.GetByCodeAsync("US");

        // Assert
        updatedCountry.Should().NotBeNull();
        updatedCountry!.Name.Should().Be("United States of America");
    }

    [Fact]
    public async Task DeleteAsync_DeletesExistingCountry()
    {
        // Act
        await _repository.DeleteAsync("UA");
        var deletedCountry = await _repository.GetByCodeAsync("UA");

        // Assert
        deletedCountry.Should().BeNull();
    }

    [Fact]
    public async Task AddAsync_DuplicateCountryCode_ThrowsException()
    {
        // Arrange
        var duplicateCountry = new Country { CountryCode = "US", Name = "United States (duplicate)" };

        // Act & Assert
        await Assert.ThrowsAsync<SqlException>(() => _repository.AddAsync(duplicateCountry));
    }

    [Theory]
    [InlineData("  ")]
    [InlineData(null)]
    [InlineData("USA")]
    public async Task AddAsync_InvalidCountryCode_ThrowsException(string invalidCountryCode)
    {
        // Arrange
        var invalidCountry = new Country { CountryCode = invalidCountryCode, Name = "Invalid" };

        // Act & Assert
        await Assert.ThrowsAsync<SqlException>(() => _repository.AddAsync(invalidCountry));
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
