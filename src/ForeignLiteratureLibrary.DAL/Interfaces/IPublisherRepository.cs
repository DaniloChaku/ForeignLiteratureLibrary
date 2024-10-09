using ForeignLiteratureLibrary.DAL.Entities;

namespace ForeignLiteratureLibrary.DAL.Interfaces;

public interface IPublisherRepository
{
    Task AddAsync(Publisher publisher);

    Task UpdateAsync(Publisher publisher);

    Task DeleteAsync(int publisherId);

    Task<Publisher?> GetByIdAsync(int publisherId);

    Task<int> GetCountAsync();

    Task<List<Publisher>> GetPageAsync(int pageNumber, int pageSize);

    Task<List<Publisher>> GetAllAsync();
}
