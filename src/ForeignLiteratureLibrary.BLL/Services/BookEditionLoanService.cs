﻿using ForeignLiteratureLibrary.BLL.Dtos;
using ForeignLiteratureLibrary.BLL.Interfaces;
using ForeignLiteratureLibrary.DAL.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForeignLiteratureLibrary.BLL.Services;

public class BookEditionLoanService : IBookEditionLoanService
{
    private readonly IBookEditionLoanRepository _bookEditionLoanRepository;

    public BookEditionLoanService(IBookEditionLoanRepository bookEditionLoanRepository)
    {
        _bookEditionLoanRepository = bookEditionLoanRepository;
    }

    public async Task AddLoanAsync(BookEditionLoanDto loanDto)
    {
        var loan = loanDto.ToEntity();
        await _bookEditionLoanRepository.AddAsync(loan);
    }

    public async Task UpdateLoanAsync(BookEditionLoanDto loanDto)
    {
        var loan = loanDto.ToEntity();
        await _bookEditionLoanRepository.UpdateAsync(loan);
    }

    public async Task DeleteLoanAsync(int loanId)
    {
        await _bookEditionLoanRepository.DeleteAsync(loanId);
    }

    public async Task<BookEditionLoanDto?> GetLoanByIdAsync(int loanId)
    {
        var loan = await _bookEditionLoanRepository.GetByIdAsync(loanId);
        return loan?.ToDto();
    }

    public async Task<PaginatedResult<BookEditionLoanDto>> GetLoansPageAsync(int pageNumber, int pageSize)
    {
        var loans = await _bookEditionLoanRepository.GetPageAsync(pageNumber, pageSize);
        var totalItems = await _bookEditionLoanRepository.GetCountAsync();

        return new PaginatedResult<BookEditionLoanDto>
        {
            Items = loans.ConvertAll(loan => loan.ToDto()),
            Page = pageNumber,
            PageSize = pageSize,
            TotalItems = totalItems
        };
    }

    public async Task<PaginatedResult<BookEditionLoanDto>> GetOverdueLoansPageAsync(int pageNumber, int pageSize)
    {
        var overdueLoans = await _bookEditionLoanRepository.GetOverduePageAsync(pageNumber, pageSize);
        var totalItems = await _bookEditionLoanRepository.GetCountAsync();  

        return new PaginatedResult<BookEditionLoanDto>
        {
            Items = overdueLoans.ConvertAll(loan => loan.ToDto()),
            Page = pageNumber,
            PageSize = pageSize,
            TotalItems = totalItems
        };
    }
}
