using ForeignLiteratureLibrary.DAL.Entities;

namespace ForeignLiteratureLibrary.DAL.Interfaces;

public interface IBookRepository
{
    Task AddAsync(Book book);

    Task UpdateAsync(Book book);

    Task DeleteAsync(int bookId);

    Task<Book?> GetByIdAsync(int bookId);

    Task<int> GetCountAsync();

    Task<List<Book>> GetPageAsync(int pageNumber, int pageSize);

    Task<List<Book>> GetAllAsync();

    Task<List<TopBook>> GetTop10BooksAsync(DateTime startDate, DateTime endDate);
}
