using ForeignLiteratureLibrary.BLL.Dtos;
using ForeignLiteratureLibrary.BLL.Interfaces;
using ForeignLiteratureLibrary.DAL.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForeignLiteratureLibrary.BLL.Services;

public class BookEditionService : IBookEditionService
{
    private readonly IBookEditionRepository _bookEditionRepository;

    public BookEditionService(IBookEditionRepository bookEditionRepository)
    {
        _bookEditionRepository = bookEditionRepository;
    }

    public async Task AddBookEditionAsync(BookEditionDto bookEditionDto)
    {
        var bookEdition = bookEditionDto.ToEntity();
        await _bookEditionRepository.AddAsync(bookEdition);
    }

    public async Task UpdateBookEditionAsync(BookEditionDto bookEditionDto)
    {
        var bookEdition = bookEditionDto.ToEntity();
        await _bookEditionRepository.UpdateAsync(bookEdition);
    }

    public async Task DeleteBookEditionAsync(int bookEditionId)
    {
        await _bookEditionRepository.DeleteAsync(bookEditionId);
    }

    public async Task<BookEditionDto?> GetBookEditionByIdAsync(int bookEditionId)
    {
        var bookEdition = await _bookEditionRepository.GetByIdAsync(bookEditionId);
        return bookEdition?.ToDto();
    }

    public async Task<PaginatedResult<BookEditionDto>> GetBookEditionsPageAsync(int pageNumber, int pageSize)
    {
        var bookEditions = await _bookEditionRepository.GetPageAsync(pageNumber, pageSize);
        var totalItems = await _bookEditionRepository.GetCountAsync();

        return new PaginatedResult<BookEditionDto>
        {
            Items = bookEditions.ConvertAll(be => be.ToDto()),
            Page = pageNumber,
            PageSize = pageSize,
            TotalItems = totalItems
        };
    }

    public async Task<PaginatedResult<BookEditionDto>> GetBookEditionsPageByGenreAsync(int genreId, int pageNumber, int pageSize)
    {
        var bookEditions = await _bookEditionRepository.GetPageByGenreAsync(genreId, pageNumber, pageSize);
        var totalItems = await _bookEditionRepository.GetCountByGenreAsync(genreId);

        return new PaginatedResult<BookEditionDto>
        {
            Items = bookEditions.ConvertAll(be => be.ToDto()),
            Page = pageNumber,
            PageSize = pageSize,
            TotalItems = totalItems
        };
    }

    public async Task<PaginatedResult<BookEditionDto>> GetBookEditionsPageByLanguageAsync(int languageId, int pageNumber, int pageSize)
    {
        var bookEditions = await _bookEditionRepository.GetPageByLanguageAsync(languageId, pageNumber, pageSize);
        var totalItems = await _bookEditionRepository.GetCountByLanguageAsync(languageId);

        return new PaginatedResult<BookEditionDto>
        {
            Items = bookEditions.ConvertAll(be => be.ToDto()),
            Page = pageNumber,
            PageSize = pageSize,
            TotalItems = totalItems
        };
    }

    public async Task<PaginatedResult<BookEditionDto>> GetBookEditionsPageByTitleAsync(string title, int pageNumber, int pageSize)
    {
        var bookEditions = await _bookEditionRepository.GetPageByTitleAsync(title, pageNumber, pageSize);
        var totalItems = await _bookEditionRepository.GetCountByTitleAsync(title);

        return new PaginatedResult<BookEditionDto>
        {
            Items = bookEditions.ConvertAll(be => be.ToDto()),
            Page = pageNumber,
            PageSize = pageSize,
            TotalItems = totalItems
        };
    }

    public async Task<PaginatedResult<BookEditionDto>> GetBookEditionsPageByIsbnAsync(string isbn, int pageNumber, int pageSize)
    {
        var bookEditions = await _bookEditionRepository.GetPageByIsbnAsync(isbn, pageNumber, pageSize);
        var totalItems = await _bookEditionRepository.GetCountByIsbnAsync(isbn);

        return new PaginatedResult<BookEditionDto>
        {
            Items = bookEditions.ConvertAll(be => be.ToDto()),
            Page = pageNumber,
            PageSize = pageSize,
            TotalItems = totalItems
        };
    }

    public async Task<List<BookEditionDto>> GetAllBookEditionsAsync()
    {
        var bookEditions = await _bookEditionRepository.GetAllAsync();
        return bookEditions.ConvertAll(be => be.ToDto());
    }
}
