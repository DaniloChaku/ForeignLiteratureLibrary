using ForeignLiteratureLibrary.DAL.Entities;

namespace ForeignLiteratureLibrary.DAL.Interfaces;

public interface IAuthorRepository
{
    Task AddAsync(Author author);

    Task UpdateAsync(Author author);

    Task DeleteAsync(int authorId);

    Task<Author?> GetByIdAsync(int authorId);

    Task<int> GetCountAsync();

    Task<List<Author>> GetPageAsync(int pageNumber, int pageSize);

    Task<List<TopAuthor>> GetTop10AuthorsAsync(DateTime startDate, DateTime endDate);
}
