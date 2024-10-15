using Dapper;
using ForeignLiteratureLibrary.DAL.Entities;
using ForeignLiteratureLibrary.DAL.Exceptions;
using ForeignLiteratureLibrary.DAL.Interfaces;
using Microsoft.Data.SqlClient;

namespace ForeignLiteratureLibrary.DAL.Repositories;

public class LanguageRepository : BaseRepository, ILanguageRepository
{
    public LanguageRepository(string connectionString) : base(connectionString)
    { }

    public async Task AddAsync(Language language)
    {
        try
        {
            const string sql = @"
            INSERT INTO Language (LanguageCode, Name)
            VALUES (@LanguageCode, @Name)";

            using var connection = await CreateConnectionAsync();
            await connection.ExecuteAsync(sql, language);
        }
        catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
        {
            throw new UniqueConstraintViolationException(
                $"Cannot add the language because this code or name already exists", ex);
        }
        catch (SqlException ex) when (ex.Number == 547)
        {
            if (ex.Message.Contains("CHK_Language_LanguageCode", StringComparison.InvariantCultureIgnoreCase))
            {
                throw new CheckConstraintViolationException(
                    "Cannot add the language because its code should be 2 or 3 characters long", ex);
            }
            if (ex.Message.Contains("CHK_Language_Name", StringComparison.InvariantCultureIgnoreCase))
            {
                throw new CheckConstraintViolationException(
                    "Cannot add the language because its Name should be at least 1 character long", ex);
            }
            throw;
        }
        catch (SqlException ex) when (ex.Number == 515)
        {
            throw new NotNullConstraintViolationException(
                "Cannot add the language because not all required columns are provided", ex);
        }
    }
    public async Task UpdateAsync(Language language)
    {
        try
        {
            const string sql = @"
            UPDATE Language 
            SET Name = @Name
            WHERE LanguageCode = @LanguageCode";

            using var connection = await CreateConnectionAsync();
            await connection.ExecuteAsync(sql, language);
        }
        catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
        {
            throw new UniqueConstraintViolationException(
                $"Cannot update language '{language.LanguageCode}' because this name already exists", ex);
        }
        catch (SqlException ex) when (ex.Number == 547)
        {
            if (ex.Message.Contains("CHK_Language_Name", StringComparison.InvariantCultureIgnoreCase))
            {
                throw new CheckConstraintViolationException(
                    "Cannot update the language because its Name should be at least 1 character long", ex);
            }
            throw;
        }
        catch (SqlException ex) when (ex.Number == 515)
        {
            throw new NotNullConstraintViolationException(
                "Cannot update the language because its name is not provided", ex);
        }
    }
    public async Task DeleteAsync(string languageCode)
    {
        try
        {
            const string sql = @"
            DELETE FROM Language 
            WHERE LanguageCode = @LanguageCode";

            using var connection = await CreateConnectionAsync();
            await connection.ExecuteAsync(sql, new { LanguageCode = languageCode });
        }
        catch (SqlException ex) when (ex.Number == 547)
        {
            throw new ForeignKeyViolationException(
                $"Cannot delete language '{languageCode}' because it is referenced by other entities.", ex);
        }
    }


    public async Task<Language?> GetByCodeAsync(string languageCode)
    {
        const string sql = @"
                SELECT LanguageCode, Name 
                FROM Language 
                WHERE LanguageCode = @LanguageCode";

        using var connection = await CreateConnectionAsync();
        return await connection.QuerySingleOrDefaultAsync<Language>(sql, new { LanguageCode = languageCode });
    }

    public async Task<int> GetCountAsync()
    {
        const string sql = "SELECT COUNT(*) FROM Language";

        using var connection = await CreateConnectionAsync();
        return await connection.ExecuteScalarAsync<int>(sql);
    }

    public async Task<List<Language>> GetPageAsync(int pageNumber, int pageSize)
    {
        const string sql = @"
                SELECT LanguageCode, Name
                FROM Language
                ORDER BY Name
                OFFSET @Offset ROWS
                FETCH NEXT @PageSize ROWS ONLY";

        using var connection = await CreateConnectionAsync();
        var languages = await connection.QueryAsync<Language>(sql,
            new
            {
                Offset = (pageNumber - 1) * pageSize,
                PageSize = pageSize
            });

        return languages.ToList();
    }

    public async Task<List<Language>> GetAllAsync()
    {
        const string sql = @"
                SELECT LanguageCode, Name
                FROM Language
                ORDER BY Name";

        using var connection = await CreateConnectionAsync();
        var languages = await connection.QueryAsync<Language>(sql);
        return languages.ToList();
    }
}
