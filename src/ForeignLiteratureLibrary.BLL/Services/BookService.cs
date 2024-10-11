using ForeignLiteratureLibrary.BLL.Dtos;
using ForeignLiteratureLibrary.BLL.Interfaces;
using ForeignLiteratureLibrary.DAL.Entities;
using ForeignLiteratureLibrary.DAL.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForeignLiteratureLibrary.BLL.Services;

public class BookService : IBookService
{
    private readonly IBookRepository _bookRepository;

    public BookService(IBookRepository bookRepository)
    {
        _bookRepository = bookRepository;
    }

    public async Task AddBookAsync(BookDto bookDto)
    {
        var book = bookDto.ToEntity();
        await _bookRepository.AddAsync(book);
    }

    public async Task UpdateBookAsync(BookDto bookDto)
    {
        var book = bookDto.ToEntity();
        await _bookRepository.UpdateAsync(book);
    }

    public async Task DeleteBookAsync(int bookId)
    {
        await _bookRepository.DeleteAsync(bookId);
    }

    public async Task<BookDto?> GetBookByIdAsync(int bookId)
    {
        var book = await _bookRepository.GetByIdAsync(bookId);
        return book?.ToDto();
    }

    public async Task<PaginatedResult<BookDto>> GetBooksPageAsync(int pageNumber, int pageSize)
    {
        var books = await _bookRepository.GetPageAsync(pageNumber, pageSize);
        var totalItems = await _bookRepository.GetCountAsync();

        return new PaginatedResult<BookDto>
        {
            Items = books.ConvertAll(b => b.ToDto()),
            Page = pageNumber,
            PageSize = pageSize,
            TotalItems = totalItems
        };
    }

    public async Task<List<BookDto>> GetAllBooksAsync()
    {
        var books = await _bookRepository.GetAllAsync();
        return books.ConvertAll(b => b.ToDto());
    }

    public async Task<List<TopBook>> GetTop10BooksAsync(DateTime startDate, DateTime endDate)
    {
        return await _bookRepository.GetTop10BooksAsync(startDate, endDate);
    }
}
