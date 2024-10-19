using ForeignLiteratureLibrary.BLL.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForeignLiteratureLibrary.BLL.Interfaces;

public interface IBookEditionLoanService
{
    Task AddLoanAsync(LoanDto loanDto);

    Task UpdateLoanAsync(LoanDto loanDto);

    Task DeleteLoanAsync(int loanId);

    Task<LoanDto?> GetLoanByIdAsync(int loanId);

    Task<PaginatedResult<LoanDto>> GetLoansPageAsync(int pageNumber, int pageSize);

    Task<PaginatedResult<LoanDto>> GetOverdueLoansPageAsync(int pageNumber, int pageSize);
}
