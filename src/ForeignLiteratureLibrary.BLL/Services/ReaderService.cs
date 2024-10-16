﻿using ForeignLiteratureLibrary.BLL.Dtos;
using ForeignLiteratureLibrary.BLL.Interfaces;
using ForeignLiteratureLibrary.DAL.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForeignLiteratureLibrary.BLL.Services;

public class ReaderService : IReaderService
{
    private readonly IReaderRepository _readerRepository;

    public ReaderService(IReaderRepository readerRepository)
    {
        _readerRepository = readerRepository;
    }

    public async Task AddReaderAsync(ReaderDto readerDto)
    {
        var reader = readerDto.ToEntity();
        await _readerRepository.AddAsync(reader);
    }

    public async Task UpdateReaderAsync(ReaderDto readerDto)
    {
        var reader = readerDto.ToEntity();
        await _readerRepository.UpdateAsync(reader);
    }

    public async Task DeleteReaderAsync(string libraryCardNumber)
    {
        await _readerRepository.DeleteAsync(libraryCardNumber);
    }

    public async Task<ReaderDto?> GetReaderByLibraryCardNumberAsync(string libraryCardNumber)
    {
        var reader = await _readerRepository.GetByLibraryCardNumberAsync(libraryCardNumber);
        return reader?.ToDto();
    }

    public async Task<List<ReaderDto>> GetReadersByFullNameAsync(string fullName)
    {
        var readers = await _readerRepository.GetByFullNameAsync(fullName);
        return readers.ConvertAll(r => r.ToDto());
    }

    public async Task<PaginatedResult<ReaderDto>> GetReadersPageAsync(int pageNumber, int pageSize)
    {
        var readers = await _readerRepository.GetPageAsync(pageNumber, pageSize);
        var totalItems = await _readerRepository.GetCountAsync();

        return new PaginatedResult<ReaderDto>
        {
            Items = readers.ConvertAll(r => r.ToDto()),
            Page = pageNumber,
            PageSize = pageSize,
            TotalItems = totalItems
        };
    }
}
