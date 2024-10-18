using ForeignLiteratureLibrary.DAL.Entities;

namespace ForeignLiteratureLibrary.DAL.Interfaces;

public interface IBookEditionRepository
{
    Task AddAsync(BookEdition bookEdition);

    Task UpdateAsync(BookEdition bookEdition);

    Task DeleteAsync(int bookEditionId);

    Task<int> GetCountAsync();

    Task<BookEdition?> GetByIdAsync(int bookEditionId);

    Task<BookEdition?> GetByIsbnAsync(string isbn);

    Task<List<BookEdition>> GetAllAsync();

    Task<List<BookEdition>> GetPageAsync(int pageNumber, int pageSize);

    Task<List<BookEdition>> GetPageByGenreAsync(int genreId);

    Task<List<BookEdition>> GetPageByLanguageAsync(string languageCode);

    Task<List<BookEdition>> GetPageByTitleAsync(string title);
}