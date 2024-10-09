using Dapper;
using ForeignLiteratureLibrary.DAL.Entities;
using ForeignLiteratureLibrary.DAL.Interfaces;

namespace ForeignLiteratureLibrary.DAL.Repositories;

public class CountryRepository : BaseRepository, ICountryRepository
{
    public CountryRepository(string connectionString) : base(connectionString)
    {
    }

    public async Task AddAsync(Country country)
    {
        const string sql = @"
                INSERT INTO Country (CountryCode, Name)
                VALUES (@CountryCode, @Name)";

        using var connection = await CreateConnectionAsync();
        await connection.ExecuteAsync(sql, country);
    }

    public async Task UpdateAsync(Country country)
    {
        const string sql = @"
                UPDATE Country 
                SET Name = @Name
                WHERE CountryCode = @CountryCode";

        using var connection = await CreateConnectionAsync();
        await connection.ExecuteAsync(sql, country);
    }

    public async Task DeleteAsync(string countryCode)
    {
        const string sql = @"
                DELETE FROM Country 
                WHERE CountryCode = @CountryCode";

        using var connection = await CreateConnectionAsync();
        await connection.ExecuteAsync(sql, new { CountryCode = countryCode });
    }

    public async Task<Country?> GetByCodeAsync(string countryCode)
    {
        const string sql = @"
                SELECT CountryCode, Name 
                FROM Country 
                WHERE CountryCode = @CountryCode";

        using var connection = await CreateConnectionAsync();
        return await connection.QuerySingleOrDefaultAsync<Country>(sql, new { CountryCode = countryCode });
    }

    public async Task<int> GetCountAsync()
    {
        const string sql = "SELECT COUNT(*) FROM Country";

        using var connection = await CreateConnectionAsync();
        return await connection.ExecuteScalarAsync<int>(sql);
    }

    public async Task<List<Country>> GetPageAsync(int pageNumber, int pageSize)
    {
        const string sql = @"
                SELECT CountryCode, Name
                FROM Country
                ORDER BY Name
                OFFSET @Offset ROWS
                FETCH NEXT @PageSize ROWS ONLY";

        using var connection = await CreateConnectionAsync();
        var countries = await connection.QueryAsync<Country>(sql,
            new
            {
                Offset = (pageNumber - 1) * pageSize,
                PageSize = pageSize
            });

        return countries.ToList();
    }

    public async Task<List<Country>> GetAllAsync()
    {
        const string sql = @"
                SELECT CountryCode, Name
                FROM Country
                ORDER BY Name";

        using var connection = await CreateConnectionAsync();
        var countries = await connection.QueryAsync<Country>(sql);
        return countries.ToList();
    }
}
