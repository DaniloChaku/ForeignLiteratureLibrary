using DALTests.TestHelpers;
using FluentAssertions;
using ForeignLiteratureLibrary.DAL.Entities;
using ForeignLiteratureLibrary.DAL.Repositories;

namespace DALTests.Repositories;

public class LoanRepositoryTests : IDisposable
{
    private readonly LoanRepository _repository;
    private readonly string _connectionString = TestConnectionStringHelper.ConnectionString;
    private bool _disposed;

    public LoanRepositoryTests()
    {
        _repository = new LoanRepository(_connectionString);
        DbSeedHelper.SeedDb(_connectionString);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingLoan_ReturnsLoanWithRelations()
    {
        // Act
        var result = await _repository.GetByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.BookEdition.Should().NotBeNull();
        result.Reader.Should().NotBeNull();
        result.ReaderID.Should().Be(1);
        result.BookEdition!.ISBN.Should().Be("978-0-14-023750-4");
    }

    [Fact]
    public async Task GetPageAsync_ReturnsCorrectPageWithRelations()
    {
        // Act
        var page = await _repository.GetPageAsync(1, 2);

        // Assert
        page.Should().HaveCount(2);
        page.Should().OnlyContain(l => l.BookEdition != null && l.Reader != null);
    }

    [Fact]
    public async Task AddAsync_AddsNewLoan()
    {
        // Arrange
        var newLoan = new Loan
        {
            BookEditionID = 3,
            ReaderID = 1,
            LoanDate = DateTime.Now,
            DueDate = DateTime.Now.AddDays(14)
        };

        // Act
        await _repository.AddAsync(newLoan);

        // Assert
        var addedLoan = await _repository.GetByIdAsync(newLoan.LoanID);
        addedLoan.Should().NotBeNull();
        addedLoan!.BookEditionID.Should().Be(newLoan.BookEditionID);
        addedLoan.ReaderID.Should().Be(newLoan.ReaderID);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesExistingLoan()
    {
        // Arrange
        var loan = await _repository.GetByIdAsync(1);
        loan!.ReturnDate = DateTime.Now;

        // Act
        await _repository.UpdateAsync(loan);
        var updatedLoan = await _repository.GetByIdAsync(1);

        // Assert
        updatedLoan.Should().NotBeNull();
        updatedLoan!.ReturnDate.Should().NotBeNull();
    }

    [Fact]
    public async Task DeleteAsync_DeletesExistingLoan()
    {
        // Arrange
        var id = 1;

        // Act
        await _repository.DeleteAsync(id);
        var deletedLoan = await _repository.GetByIdAsync(id);

        // Assert
        deletedLoan.Should().BeNull();
    }

    [Fact]
    public async Task GetOverduePageAsync_ReturnsOnlyOverdueLoans()
    {
        // Act
        var overdueLoans = await _repository.GetOverduePageAsync(1, 10);

        // Assert
        overdueLoans.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetCountAsync_ReturnsCorrectCount()
    {
        // Act
        var count = await _repository.GetCountAsync();

        // Assert
        count.Should().Be(2);
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