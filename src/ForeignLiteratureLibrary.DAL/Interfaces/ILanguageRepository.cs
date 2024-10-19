using ForeignLiteratureLibrary.DAL.Entities;

namespace ForeignLiteratureLibrary.DAL.Interfaces;

public interface ILanguageRepository
{
    Task AddAsync(Language language);

    Task UpdateAsync(Language language);

    Task DeleteAsync(int languageId);

    Task<Language?> GetByIdAsync(int languageId);

    Task<int> GetCountAsync();

    Task<List<Language>> GetPageAsync(int pageNumber, int pageSize);

    Task<List<Language>> GetAllAsync();
}
