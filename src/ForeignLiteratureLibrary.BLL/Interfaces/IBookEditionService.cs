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

    Task<BookEditionDto?> GetBookEditionByIsbnAsync(string isbn);

    Task<PaginatedResult<BookEditionDto>> GetBookEditionsPageAsync(int pageNumber, int pageSize);

    Task<List<BookEditionDto>> GetBookEditionsByGenreAsync(int genreId);

    Task<List<BookEditionDto>> GetBookEditionsByLanguageAsync(string languageCode);

    Task<List<BookEditionDto>> GetBookEditionsByTitleAsync(string title);
}
