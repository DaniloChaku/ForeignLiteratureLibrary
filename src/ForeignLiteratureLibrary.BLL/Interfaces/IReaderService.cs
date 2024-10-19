using ForeignLiteratureLibrary.BLL.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForeignLiteratureLibrary.BLL.Interfaces;

public interface IReaderService
{
    Task AddReaderAsync(ReaderDto readerDto);

    Task UpdateReaderAsync(ReaderDto readerDto);

    Task DeleteReaderAsync(int readerId);

    Task<ReaderDto?> GetReaderByIdAsync(int readerId);

    Task<List<ReaderDto>> GetReadersByFullNameAsync(string fullName);

    Task<List<ReaderDto>> GetReadersByLibraryCardNumberAsync(string libraryCardNumber);

    Task<PaginatedResult<ReaderDto>> GetReadersPageAsync(int pageNumber, int pageSize);

    Task<List<ReaderDto>> GetAllReadersAsync();
}   
