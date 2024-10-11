using ForeignLiteratureLibrary.BLL.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForeignLiteratureLibrary.BLL.Interfaces;

public interface IBookEditionLoanService
{
    Task AddLoanAsync(BookEditionLoanDto loanDto);

    Task UpdateLoanAsync(BookEditionLoanDto loanDto);

    Task DeleteLoanAsync(int loanId);

    Task<BookEditionLoanDto?> GetLoanByIdAsync(int loanId);

    Task<PaginatedResult<BookEditionLoanDto>> GetLoansPageAsync(int pageNumber, int pageSize);

    Task<PaginatedResult<BookEditionLoanDto>> GetOverdueLoansPageAsync(int pageNumber, int pageSize);
}
