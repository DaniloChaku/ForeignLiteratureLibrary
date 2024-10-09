using ForeignLiteratureLibrary.DAL.Entities;

namespace ForeignLiteratureLibrary.DAL.Interfaces;

public interface IGenreRepository
{
    Task AddAsync(Genre genre);

    Task UpdateAsync(Genre genre);

    Task DeleteAsync(int genreId);

    Task<Genre?> GetByIdAsync(int genreId);

    Task<int> GetCountAsync();

    Task<List<Genre>> GetPageAsync(int pageNumber, int pageSize);

    Task<List<Genre>> GetAllAsync();
}
