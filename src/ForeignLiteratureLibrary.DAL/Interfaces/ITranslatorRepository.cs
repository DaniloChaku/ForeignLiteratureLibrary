using ForeignLiteratureLibrary.DAL.Entities;

namespace ForeignLiteratureLibrary.DAL.Interfaces;

public interface ITranslatorRepository
{
    Task AddAsync(Translator translator);

    Task UpdateAsync(Translator translator);

    Task DeleteAsync(int translatorId);

    Task<Translator?> GetByIdWithBookEditionsAsync(int translatorId);

    Task<int> GetCountAsync();

    Task<List<Translator>> GetPageAsync(int pageNumber, int pageSize);
}
