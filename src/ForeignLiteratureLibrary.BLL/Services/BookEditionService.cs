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

    public async Task<BookEditionDto?> GetBookEditionByIsbnAsync(string isbn)
    {
        var bookEdition = await _bookEditionRepository.GetByIsbnAsync(isbn);
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

    public async Task<List<BookEditionDto>> GetBookEditionsByGenreAsync(int genreId)
    {
        var bookEditions = await _bookEditionRepository.GetPageByGenreAsync(genreId);
        return bookEditions.ConvertAll(be => be.ToDto());
    }

    public async Task<List<BookEditionDto>> GetBookEditionsByLanguageAsync(string languageCode)
    {
        var bookEditions = await _bookEditionRepository.GetPageByLanguageAsync(languageCode);
        return bookEditions.ConvertAll(be => be.ToDto());
    }

    public async Task<List<BookEditionDto>> GetBookEditionsByTitleAsync(string title)
    {
        var bookEditions = await _bookEditionRepository.GetPageByTitleAsync(title);
        return bookEditions.ConvertAll(be => be.ToDto());
    }

    public async Task<List<BookEditionDto>> GetAllBookEditionsAsync()
    {
        var bookEditions = await _bookEditionRepository.GetAllAsync();
        return bookEditions.ConvertAll(be => be.ToDto());
    }
}
