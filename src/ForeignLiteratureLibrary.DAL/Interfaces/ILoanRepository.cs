using ForeignLiteratureLibrary.DAL.Entities;

namespace ForeignLiteratureLibrary.DAL.Interfaces;

public interface ILoanRepository
{
    Task AddAsync(Loan loan);

    Task UpdateAsync(Loan loan);

    Task DeleteAsync(int loanId);

    Task<Loan?> GetByIdAsync(int loanId);

    Task<int> GetCountAsync();

    Task<List<Loan>> GetPageAsync(int pageNumber, int pageSize);

    Task<List<Loan>> GetOverduePageAsync(int pageNumber, int pageSize);
}
