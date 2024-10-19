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
            INSERT INTO Country (CountryName)
            VALUES (@CountryName)";

            using var connection = await CreateConnectionAsync();
            await connection.ExecuteAsync(sql, country);
        }
        catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
        {
            throw new UniqueConstraintViolationException(
                $"Cannot add the country because this code or name already exist", ex);
        }
        catch (SqlException ex) when (ex.Number == 547 && ex.Message.Contains("CHK_Country_CountryName"))
        {
            throw new CheckConstraintViolationException(
                "Cannot add the country because its Name should be at least 1 character long", ex);
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
            SET CountryName = @CountryName
            WHERE CountryID = @CountryID";

            using var connection = await CreateConnectionAsync();
            await connection.ExecuteAsync(sql, country);
        }
        catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
        {
            throw new UniqueConstraintViolationException(
                $"Cannot update the country because this name already exists", ex);
        }
        catch (SqlException ex) when (ex.Number == 547 && ex.Message.Contains("CHK_Country_CountryName"))
        {
            throw new CheckConstraintViolationException(
                "Cannot update the country because its Name should be at least 1 character long", ex);
        }
        catch (SqlException ex) when (ex.Number == 515)
        {
            throw new NotNullConstraintViolationException(
                "Cannot update the country because its name is not provided", ex);
        }
    }

    public async Task DeleteAsync(int countryId)
    {
        try
        {
            const string sql = @"
            DELETE FROM Country 
            WHERE CountryID = @CountryID";

            using var connection = await CreateConnectionAsync();
            await connection.ExecuteAsync(sql, new { CountryID = countryId });
        }
        catch (SqlException ex) when (ex.Number == 547)
        {
            throw new ForeignKeyViolationException(
                $"Cannot delete country '{countryId}' because it is referenced by other entities.", ex);
        }
    }

    public async Task<Country?> GetByIdAsync(int countryId)
    {
        const string sql = @"
            SELECT CountryID, CountryName 
            FROM Country 
            WHERE CountryID = @CountryID";

        using var connection = await CreateConnectionAsync();
        return await connection.QuerySingleOrDefaultAsync<Country>(sql, new { CountryID = countryId });
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
            SELECT CountryID, CountryName
            FROM Country
            ORDER BY CountryName
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
            SELECT CountryID, CountryName
            FROM Country
            ORDER BY CountryName";

        using var connection = await CreateConnectionAsync();
        var countries = await connection.QueryAsync<Country>(sql);
        return countries.ToList();
    }
}
