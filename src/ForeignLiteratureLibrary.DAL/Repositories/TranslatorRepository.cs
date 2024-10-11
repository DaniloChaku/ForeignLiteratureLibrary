using Dapper;
using ForeignLiteratureLibrary.DAL.Entities;
using ForeignLiteratureLibrary.DAL.Interfaces;

namespace ForeignLiteratureLibrary.DAL.Repositories;

public class TranslatorRepository : BaseRepository, ITranslatorRepository
{
    public TranslatorRepository(string connectionString) : base(connectionString)
    {
    }

    public async Task AddAsync(Translator translator)
    {
        const string sql = @"
                INSERT INTO Translator (FullName)
                OUTPUT INSERTED.TranslatorID
                VALUES (@FullName)";

        using var connection = await CreateConnectionAsync();
        var translatorId = await connection.ExecuteScalarAsync<int>(sql, translator);
        translator.TranslatorID = translatorId;
    }

    public async Task UpdateAsync(Translator translator)
    {
        const string sql = @"
                UPDATE Translator 
                SET FullName = @FullName
                WHERE TranslatorID = @TranslatorId";

        using var connection = await CreateConnectionAsync();
        await connection.ExecuteAsync(sql, translator);
    }

    public async Task DeleteAsync(int translatorId)
    {
        const string sql = @"
                DELETE FROM BookEditionTranslator WHERE TranslatorID = @TranslatorId;
                DELETE FROM Translator WHERE TranslatorID = @TranslatorId";

        using var connection = await CreateConnectionAsync();
        await connection.ExecuteAsync(sql, new { TranslatorId = translatorId });
    }

    public async Task<Translator?> GetByIdAsync(int translatorId)
    {
        const string sql = @"
                SELECT t.TranslatorID, t.FullName, be.BookEditionID, 
                       be.ISBN, be.Title as Title
                FROM Translator t
                LEFT JOIN BookEditionTranslator bet ON t.TranslatorID = bet.TranslatorID
                LEFT JOIN BookEdition be ON bet.BookEditionID = be.BookEditionID
                WHERE t.TranslatorID = @TranslatorId";

        using var connection = await CreateConnectionAsync();

        Dictionary<int, Translator> translatorMap = [];
        await connection.QueryAsync<Translator, BookEdition, Translator>(
            sql,
            (translator, bookEdition) =>
            {
                if (!translatorMap.TryGetValue(translator.TranslatorID, out var translatorEntry))
                {
                    translatorEntry = translator;
                    translatorEntry.BookEditions = [];
                    translatorMap.Add(translator.TranslatorID, translatorEntry);
                }

                if (bookEdition != null)
                {
                    translatorEntry.BookEditions.Add(bookEdition);
                }

                return translatorEntry;
            },
            new { TranslatorId = translatorId },
            splitOn: "BookEditionID");

        return translatorMap.Values.FirstOrDefault();
    }

    public async Task<int> GetCountAsync()
    {
        const string sql = "SELECT COUNT(*) FROM Translator";

        using var connection = await CreateConnectionAsync();
        return await connection.ExecuteScalarAsync<int>(sql);
    }

    public async Task<List<Translator>> GetPageAsync(int pageNumber, int pageSize)
    {
        const string sql = @"
                SELECT t.TranslatorID, t.FullName
                FROM Translator t
                ORDER BY t.FullName
                OFFSET @Offset ROWS
                FETCH NEXT @PageSize ROWS ONLY";

        using var connection = await CreateConnectionAsync();
        var parameters = new { Offset = (pageNumber - 1) * pageSize, PageSize = pageSize };
        var translators = await connection.QueryAsync<Translator>(sql, parameters);

        return translators.ToList();
    }

    public async Task<List<Translator>> GetAllAsync()
    {
        const string sql = @"
                SELECT TranslatorID, FullName
                FROM Translator
                ORDER BY FullName";

        using var connection = await CreateConnectionAsync();
        var translators = await connection.QueryAsync<Translator>(sql);

        return translators.ToList();
    }
}
