using ForeignLiteratureLibrary.BLL.Dtos;
using ForeignLiteratureLibrary.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForeignLiteratureLibrary.BLL.Interfaces;

public interface IBookService
{
    Task AddBookAsync(BookDto bookDto);

    Task UpdateBookAsync(BookDto bookDto);

    Task DeleteBookAsync(int bookId);

    Task<BookDto?> GetBookByIdAsync(int bookId);

    Task<PaginatedResult<BookDto>> GetBooksPageAsync(int pageNumber, int pageSize);

    Task<List<BookDto>> GetAllBooksAsync();

    Task<List<TopBook>> GetTop10BooksAsync(DateTime startDate, DateTime endDate);
}
