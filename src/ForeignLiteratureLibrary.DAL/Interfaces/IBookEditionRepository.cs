using ForeignLiteratureLibrary.DAL.Entities;

namespace ForeignLiteratureLibrary.DAL.Interfaces;

public interface IBookEditionRepository
{
    Task AddAsync(BookEdition bookEdition);

    Task UpdateAsync(BookEdition bookEdition);

    Task DeleteAsync(int bookEditionId);

    Task<int> GetCountAsync();

    Task<BookEdition?> GetByIdAsync(int bookEditionId);

    Task<List<BookEdition>> GetAllAsync();

    Task<List<BookEdition>> GetPageAsync(int pageNumber, int pageSize);

    Task<List<BookEdition>> GetPageByGenreAsync(int genreId, int pageNumber, int pageSize);

    Task<int> GetCountByGenreAsync(int genreId);

    Task<List<BookEdition>> GetPageByLanguageAsync(int languageId, int pageNumber, int pageSize);

    Task<int> GetCountByLanguageAsync(int languageId);

    Task<List<BookEdition>> GetPageByTitleAsync(string title, int pageNumber, int pageSize);

    Task<int> GetCountByTitleAsync(string title);

    Task<List<BookEdition>> GetPageByIsbnAsync(string isbn, int pageNumber, int pageSize);

    Task<int> GetCountByIsbnAsync(string isbn);
}