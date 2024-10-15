using Dapper;
using ForeignLiteratureLibrary.DAL.Entities;
using ForeignLiteratureLibrary.DAL.Exceptions;
using ForeignLiteratureLibrary.DAL.Interfaces;
using Microsoft.Data.SqlClient;

namespace ForeignLiteratureLibrary.DAL.Repositories;

public class CountryRepository : BaseRepository, ICountryRepository
{
    public CountryRepository(string connectionString) : base(connectionString)
    {
    }

    public async Task AddAsync(Country country)
    {
        try
        {
            const string sql = @"
                INSERT INTO Country (CountryCode, Name)
                VALUES (@CountryCode, @Name)";

            using var connection = await CreateConnectionAsync();
            await connection.ExecuteAsync(sql, country);
        }
        catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
        {
            throw new UniqueConstraintViolationException(
                $"Cannot add the country because this code or name already exist", ex);
        }
        catch (SqlException ex) when (ex.Number == 547)
        {
            if (ex.Message.Contains("CHK_Country_CountryCode", StringComparison.InvariantCultureIgnoreCase))
            {
                throw new CheckConstraintViolationException(
                    "Cannot add the country because its code should be 2 or 3 characters long", ex);
            }
            if (ex.Message.Contains("CHK_Country_Name", StringComparison.InvariantCultureIgnoreCase))
            {
                throw new CheckConstraintViolationException(
                    "Cannot add the country because its Name should be at least 1 character long", ex);
            }
            throw;
        }
        catch (SqlException ex) when (ex.Number == 515)
        {
            throw new NotNullConstraintViolationException(
                "Cannot add the country because a required field is missing", ex);
        }
    }

    public async Task UpdateAsync(Country country)
    {
        try
        {
            const string sql = @"
                UPDATE Country 
                SET Name = @Name
                WHERE CountryCode = @CountryCode";

            using var connection = await CreateConnectionAsync();
            await connection.ExecuteAsync(sql, country);
        }
        catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
        {
            throw new UniqueConstraintViolationException(
                $"Cannot update country '{country.CountryCode}' because this name already exists", ex);
        }
        catch (SqlException ex) when (ex.Number == 547)
        {
            if (ex.Message.Contains("CHK_Country_Name", StringComparison.InvariantCultureIgnoreCase))
            {
                throw new CheckConstraintViolationException(
                    "Cannot add the country because its Name should be at least 1 character long", ex);
            }
            throw;
        }
        catch (SqlException ex) when (ex.Number == 515)
        {
            throw new NotNullConstraintViolationException(
                "Cannot add the country because its name is not provided", ex);
        }
    }

    public async Task DeleteAsync(string countryCode)
    {
        try
        {
            const string sql = @"
                DELETE FROM Country 
                WHERE CountryCode = @CountryCode";

            using var connection = await CreateConnectionAsync();
            await connection.ExecuteAsync(sql, new { CountryCode = countryCode });
        }
        catch (SqlException ex) when (ex.Number == 547)
        {
            throw new ForeignKeyViolationException(
                $"Cannot delete country '{countryCode}' because it is referenced by other entities.", ex);
        }
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
