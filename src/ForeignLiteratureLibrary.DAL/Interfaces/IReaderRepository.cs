using ForeignLiteratureLibrary.DAL.Entities;

namespace ForeignLiteratureLibrary.DAL.Interfaces;

public interface IReaderRepository
{
    Task AddAsync(Reader reader);

    Task UpdateAsync(Reader reader);

    Task DeleteAsync(string libraryCardNumber);

    Task<Reader?> GetByLibraryCardNumberAsync(string libraryCardNumber);

    Task<int> GetCountAsync();

    Task<List<Reader>> GetByFullNameAsync(string fullName);

    Task<List<Reader>> GetPageAsync(int pageNumber, int pageSize);

    Task<List<Reader>> GetAllAsync();
}
