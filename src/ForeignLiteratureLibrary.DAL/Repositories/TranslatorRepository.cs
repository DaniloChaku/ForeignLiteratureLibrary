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
                INSERT INTO Translator (FullName, CountryCode)
                OUTPUT INSERTED.TranslatorID
                VALUES (@FullName, @CountryCode)";

        using var connection = await CreateConnectionAsync();
        var translatorId = await connection.ExecuteScalarAsync<int>(sql, translator);
        translator.TranslatorID = translatorId;
    }

    public async Task UpdateAsync(Translator translator)
    {
        const string sql = @"
                UPDATE Translator 
                SET FullName = @FullName, CountryCode = @CountryCode
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

    public async Task<Translator?> GetByIdWithBookEditionsAsync(int translatorId)
    {
        const string sql = @"
                SELECT t.TranslatorID, t.FullName, t.CountryCode, 
                       c.CountryCode, c.Name, be.BookEditionID, 
                       be.ISBN, be.Title as Title
                FROM Translator t
                JOIN Country c ON t.CountryCode = c.CountryCode
                LEFT JOIN BookEditionTranslator bet ON t.TranslatorID = bet.TranslatorID
                LEFT JOIN BookEdition be ON bet.BookEditionID = be.BookEditionID
                WHERE t.TranslatorID = @TranslatorId";

        using var connection = await CreateConnectionAsync();

        Dictionary<int, Translator> translatorMap = [];
        await connection.QueryAsync<Translator, Country, BookEdition, Translator>(
            sql,
            (translator, country, bookEdition) =>
            {
                if (!translatorMap.TryGetValue(translator.TranslatorID, out var translatorEntry))
                {
                    translatorEntry = translator;
                    translatorEntry.Country = country;
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
            splitOn: "CountryCode,BookEditionID");

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
                SELECT t.TranslatorID, t.FullName, t.CountryCode,
                       c.CountryCode, c.Name as CountryName
                FROM Translator t
                JOIN Country c ON t.CountryCode = c.CountryCode
                ORDER BY t.FullName
                OFFSET @Offset ROWS
                FETCH NEXT @PageSize ROWS ONLY";

        using var connection = await CreateConnectionAsync();
        var translators = await connection.QueryAsync<Translator, Country, Translator>(
            sql,
            (translator, country) =>
            {
                translator.Country = country;
                return translator;
            },
            new
            {
                Offset = (pageNumber - 1) * pageSize,
                PageSize = pageSize
            },
            splitOn: "CountryCode");

        return translators.ToList();
    }
}
