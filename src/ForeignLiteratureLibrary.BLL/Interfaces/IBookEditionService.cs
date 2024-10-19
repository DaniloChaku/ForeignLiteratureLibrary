using ForeignLiteratureLibrary.BLL.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForeignLiteratureLibrary.BLL.Interfaces;

public interface IBookEditionService
{
    Task AddBookEditionAsync(BookEditionDto bookEditionDto);

    Task UpdateBookEditionAsync(BookEditionDto bookEditionDto);

    Task DeleteBookEditionAsync(int bookEditionId);

    Task<BookEditionDto?> GetBookEditionByIdAsync(int bookEditionId);

    Task<List<BookEditionDto>> GetAllBookEditionsAsync();

    Task<PaginatedResult<BookEditionDto>> GetBookEditionsPageAsync(int pageNumber, int pageSize);

    Task<PaginatedResult<BookEditionDto>> GetBookEditionsPageByGenreAsync(int genreId, int pageNumber, int pageSize);

    Task<PaginatedResult<BookEditionDto>> GetBookEditionsPageByLanguageAsync(int languageId, int pageNumber, int pageSize);

    Task<PaginatedResult<BookEditionDto>> GetBookEditionsPageByTitleAsync(string title, int pageNumber, int pageSize);

    Task<PaginatedResult<BookEditionDto>> GetBookEditionsPageByIsbnAsync(string isbn, int pageNumber, int pageSize);
}
