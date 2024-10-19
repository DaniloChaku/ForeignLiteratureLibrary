using Dapper;
using ForeignLiteratureLibrary.DAL.Entities;
using ForeignLiteratureLibrary.DAL.Exceptions;
using ForeignLiteratureLibrary.DAL.Interfaces;
using Microsoft.Data.SqlClient;

namespace ForeignLiteratureLibrary.DAL.Repositories;

public class TranslatorRepository : BaseRepository, ITranslatorRepository
{
    public TranslatorRepository(string connectionString) : base(connectionString)
    {
    }

    public async Task AddAsync(Translator translator)
    {
        try
        {
            const string sql = @"
            INSERT INTO Translator (TranslatorFullName, CountryID)
            OUTPUT INSERTED.TranslatorID
            VALUES (@TranslatorFullName, @CountryID)";

            using var connection = await CreateConnectionAsync();
            translator.TranslatorID = await connection.ExecuteScalarAsync<int>(sql, translator);
        }
        catch (SqlException ex) when (ex.Number == 547 && ex.Message.Contains("CHK_Translator_TranslatorFullName"))
        {
            throw new CheckConstraintViolationException(
                "Cannot add the translator because the full name cannot be empty", ex);
        }
        catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
        {
            throw new UniqueConstraintViolationException(
                "Cannot add the translator because a record with the same name already exists", ex);
        }
        catch (SqlException ex) when (ex.Number == 515)
        {
            throw new NotNullConstraintViolationException(
                "Cannot add the translator because a required field is missing", ex);
        }
    }

    public async Task UpdateAsync(Translator translator)
    {
        try
        {
            const string sql = @"
            UPDATE Translator 
            SET TranslatorFullName = @TranslatorFullName,
                CountryID = @CountryID
            WHERE TranslatorID = @TranslatorID";

            using var connection = await CreateConnectionAsync();
            await connection.ExecuteAsync(sql, translator);
        }
        catch (SqlException ex) when (ex.Number == 547 && ex.Message.Contains("CHK_Translator_TranslatorFullName"))
        {
            throw new CheckConstraintViolationException(
                "Cannot update the translator because the full name cannot be empty", ex);
        }
        catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
        {
            throw new UniqueConstraintViolationException(
                "Cannot update the translator because a record with the same name already exists", ex);
        }
        catch (SqlException ex) when (ex.Number == 515)
        {
            throw new NotNullConstraintViolationException(
                "Cannot update the translator because a required field is missing", ex);
        }
    }

    public async Task DeleteAsync(int translatorId)
    {
        try
        {
            const string sql = @"
                DELETE FROM BookEditionTranslator WHERE TranslatorID = @TranslatorId;
                DELETE FROM Translator WHERE TranslatorID = @TranslatorId";

            using var connection = await CreateConnectionAsync();
            await connection.ExecuteAsync(sql, new { TranslatorId = translatorId });
        }
        catch (SqlException ex) when (ex.Number == 547)
        {
            throw new ForeignKeyViolationException(
                $"Cannot delete the translator '{translatorId}' because they are referenced in other records.", ex);
        }
    }

    public async Task<Translator?> GetByIdAsync(int translatorId)
    {
        const string sql = @"
        SELECT t.TranslatorID, t.TranslatorFullName, t.CountryID, c.CountryID, c.CountryName, be.*
        FROM Translator t
        LEFT JOIN Country c ON t.CountryID = c.CountryID
        LEFT JOIN BookEditionTranslator bet ON t.TranslatorID = bet.TranslatorID
        LEFT JOIN BookEdition be ON bet.BookEditionID = be.BookEditionID
        WHERE t.TranslatorID = @TranslatorID";

        using var connection = await CreateConnectionAsync();

        var translatorMap = new Dictionary<int, Translator>();
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
            new { TranslatorID = translatorId },
            splitOn: "CountryID,BookEditionID");

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
        SELECT t.TranslatorID, t.TranslatorFullName, t.CountryID, c.CountryID, c.CountryName
        FROM Translator t
        LEFT JOIN Country c ON t.CountryID = c.CountryID
        ORDER BY t.TranslatorFullName
        OFFSET @Offset ROWS
        FETCH NEXT @PageSize ROWS ONLY";

        using var connection = await CreateConnectionAsync();
        var parameters = new { Offset = (pageNumber - 1) * pageSize, PageSize = pageSize };
        var translators = await connection.QueryAsync<Translator, Country, Translator>(
            sql,
            (translator, country) =>
            {
                translator.Country = country;
                return translator;
            },
            parameters,
            splitOn: "CountryID");

        return translators.ToList();
    }

    public async Task<List<Translator>> GetAllAsync()
    {
        const string sql = @"
        SELECT t.TranslatorID, t.TranslatorFullName, t.CountryID, c.CountryID, c.CountryName
        FROM Translator t
        LEFT JOIN Country c ON t.CountryID = c.CountryID
        ORDER BY t.TranslatorFullName";

        using var connection = await CreateConnectionAsync();
        var translators = await connection.QueryAsync<Translator, Country, Translator>(
            sql,
            (translator, country) =>
            {
                translator.Country = country;
                return translator;
            },
            splitOn: "CountryID");

        return translators.ToList();
    }
}
