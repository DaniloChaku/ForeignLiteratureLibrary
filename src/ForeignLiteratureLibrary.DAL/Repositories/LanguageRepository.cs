using Dapper;
using ForeignLiteratureLibrary.DAL.Entities;
using ForeignLiteratureLibrary.DAL.Interfaces;

namespace ForeignLiteratureLibrary.DAL.Repositories;

public class LanguageRepository : BaseRepository, ILanguageRepository
{
    public LanguageRepository(string connectionString) : base(connectionString)
    { }

    public async Task AddAsync(Language language)
    {
        const string sql = @"
                INSERT INTO Language (LanguageCode, Name)
                VALUES (@LanguageCode, @Name)";

        using var connection = await CreateConnectionAsync();
        await connection.ExecuteAsync(sql, language);
    }

    public async Task UpdateAsync(Language language)
    {
        const string sql = @"
                UPDATE Language 
                SET Name = @Name
                WHERE LanguageCode = @LanguageCode";

        using var connection = await CreateConnectionAsync();
        await connection.ExecuteAsync(sql, language);
    }

    public async Task DeleteAsync(string languageCode)
    {
        const string sql = @"
                DELETE FROM Language 
                WHERE LanguageCode = @LanguageCode";

        using var connection = await CreateConnectionAsync();
        await connection.ExecuteAsync(sql, new { LanguageCode = languageCode });
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
