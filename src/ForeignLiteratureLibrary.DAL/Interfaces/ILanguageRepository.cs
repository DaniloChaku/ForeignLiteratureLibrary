using ForeignLiteratureLibrary.DAL.Entities;

namespace ForeignLiteratureLibrary.DAL.Interfaces;

public interface ILanguageRepository
{
    Task AddAsync(Language language);

    Task UpdateAsync(Language language);

    Task DeleteAsync(string languageCode);

    Task<Language?> GetByCodeAsync(string languageCode);

    Task<int> GetCountAsync();

    Task<List<Language>> GetPageAsync(int pageNumber, int pageSize);

    Task<List<Language>> GetAllAsync();
}
