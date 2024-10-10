using ForeignLiteratureLibrary.DAL.Entities;

namespace ForeignLiteratureLibrary.DAL.Interfaces;

public interface ITranslatorRepository
{
    Task AddAsync(Translator translator);

    Task UpdateAsync(Translator translator);

    Task DeleteAsync(int translatorId);

    Task<Translator?> GetByIdAsync(int translatorId);

    Task<int> GetCountAsync();

    Task<List<Translator>> GetAllAsync();

    Task<List<Translator>> GetPageAsync(int pageNumber, int pageSize);
}
