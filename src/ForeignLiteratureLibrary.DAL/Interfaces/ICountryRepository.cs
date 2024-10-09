using ForeignLiteratureLibrary.DAL.Entities;

namespace ForeignLiteratureLibrary.DAL.Interfaces;

public interface ICountryRepository
{
    Task AddAsync(Country country);

    Task UpdateAsync(Country country);

    Task DeleteAsync(string countryCode);

    Task<Country?> GetByCodeAsync(string countryCode);

    Task<int> GetCountAsync();

    Task<List<Country>> GetPageAsync(int pageNumber, int pageSize);

    Task<List<Country>> GetAllAsync();
}
