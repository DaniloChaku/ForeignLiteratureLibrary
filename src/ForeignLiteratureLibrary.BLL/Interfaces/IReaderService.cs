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

    Task DeleteReaderAsync(string libraryCardNumber);

    Task<ReaderDto?> GetReaderByLibraryCardNumberAsync(string libraryCardNumber);

    Task<List<ReaderDto>> GetReadersByFullNameAsync(string fullName);

    Task<PaginatedResult<ReaderDto>> GetReadersPageAsync(int pageNumber, int pageSize);
}   
