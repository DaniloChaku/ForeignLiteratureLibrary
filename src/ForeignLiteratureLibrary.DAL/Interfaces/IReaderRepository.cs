using ForeignLiteratureLibrary.DAL.Entities;

namespace ForeignLiteratureLibrary.DAL.Interfaces;

public interface IReaderRepository
{
    Task AddAsync(Reader reader);

    Task UpdateAsync(Reader reader);

    Task DeleteAsync(int readerId);

    Task<Reader?> GetByIdAsync(int readerId);

    Task<int> GetCountAsync();

    Task<List<Reader>> GetByLibraryCardNumberAsync(string libraryCardNumber);

    Task<List<Reader>> GetByFullNameAsync(string readerFullName);

    Task<List<Reader>> GetPageAsync(int pageNumber, int pageSize);

    Task<List<Reader>> GetAllAsync();
}
