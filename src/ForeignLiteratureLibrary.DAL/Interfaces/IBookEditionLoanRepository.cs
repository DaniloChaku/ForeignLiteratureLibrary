using ForeignLiteratureLibrary.DAL.Entities;

namespace ForeignLiteratureLibrary.DAL.Interfaces;

public interface IBookEditionLoanRepository
{
    Task AddAsync(BookEditionLoan loan);

    Task UpdateAsync(BookEditionLoan loan);

    Task DeleteAsync(int loanId);

    Task<BookEditionLoan?> GetByIdAsync(int loanId);

    Task<int> GetCountAsync();

    Task<List<BookEditionLoan>> GetPageAsync(int pageNumber, int pageSize);

    Task<List<BookEditionLoan>> GetOverduePageAsync(int pageNumber, int pageSize);
}
