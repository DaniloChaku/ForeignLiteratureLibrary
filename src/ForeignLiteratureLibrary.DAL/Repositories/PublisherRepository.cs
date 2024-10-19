using Dapper;
using ForeignLiteratureLibrary.DAL.Entities;
using ForeignLiteratureLibrary.DAL.Exceptions;
using ForeignLiteratureLibrary.DAL.Interfaces;
using Microsoft.Data.SqlClient;

namespace ForeignLiteratureLibrary.DAL.Repositories;

public class PublisherRepository : BaseRepository, IPublisherRepository
{
    public PublisherRepository(string connectionString) : base(connectionString)
    {
    }

    public async Task AddAsync(Publisher publisher)
    {
        try
        {
            const string sql = @"
        INSERT INTO Publisher (PublisherName, CountryID)
        OUTPUT INSERTED.PublisherID
        VALUES (@PublisherName, @CountryID)";

            using var connection = await CreateConnectionAsync();
            var publisherId = await connection.ExecuteScalarAsync<int>(sql, publisher);
            publisher.PublisherID = publisherId;
        }
        catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
        {
            throw new UniqueConstraintViolationException(
                $"Cannot add the publisher because this name already exists", ex);
        }
        catch (SqlException ex) when (ex.Number == 547)
        {
            if (ex.Message.Contains("CHK_Publisher_PublisherName", StringComparison.InvariantCultureIgnoreCase))
            {
                throw new CheckConstraintViolationException(
                    "Cannot add the publisher because its name should be at least 1 character long", ex);
            }
            throw;
        }
        catch (SqlException ex) when (ex.Number == 515)
        {
            throw new NotNullConstraintViolationException(
                "Cannot add the publisher because its name or country is not provided", ex);
        }
    }

    public async Task UpdateAsync(Publisher publisher)
    {
        try
        {
            const string sql = @"
        UPDATE Publisher 
        SET PublisherName = @PublisherName, CountryID = @CountryID
        WHERE PublisherID = @PublisherID";

            using var connection = await CreateConnectionAsync();
            await connection.ExecuteAsync(sql, publisher);
        }
        catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
        {
            throw new UniqueConstraintViolationException(
                $"Cannot update publisher '{publisher.PublisherID}' because this name already exists", ex);
        }
        catch (SqlException ex) when (ex.Number == 547)
        {
            if (ex.Message.Contains("CHK_Publisher_PublisherName", StringComparison.InvariantCultureIgnoreCase))
            {
                throw new CheckConstraintViolationException(
                    "Cannot update the publisher because its name should be at least 1 character long", ex);
            }
            throw;
        }
        catch (SqlException ex) when (ex.Number == 515)
        {
            throw new NotNullConstraintViolationException(
                "Cannot update the publisher because its name or country is not provided", ex);
        }
    }

    public async Task DeleteAsync(int publisherId)
    {
        try
        {
            const string sql = @"
        DELETE FROM Publisher 
        WHERE PublisherID = @PublisherID";

            using var connection = await CreateConnectionAsync();
            await connection.ExecuteAsync(sql, new { PublisherID = publisherId });
        }
        catch (SqlException ex) when (ex.Number == 547)
        {
            throw new ForeignKeyViolationException(
                $"Cannot delete publisher '{publisherId}' because it is referenced by other entities.", ex);
        }
    }

    public async Task<Publisher?> GetByIdAsync(int publisherId)
    {
        const string sql = @"
        SELECT p.PublisherID, p.PublisherName, p.CountryID, c.CountryID, c.CountryName
        FROM Publisher p
        LEFT JOIN Country c ON p.CountryID = c.CountryID
        WHERE p.PublisherID = @PublisherID";

        using var connection = await CreateConnectionAsync();
        var countries = await connection.QueryAsync<Publisher, Country, Publisher>(
            sql,
            (publisher, country) =>
            {
                publisher.Country = country;
                return publisher;
            },
            new { PublisherID = publisherId },
            splitOn: "CountryID");

        return countries.FirstOrDefault();
    }

    public async Task<int> GetCountAsync()
    {
        const string sql = "SELECT COUNT(*) FROM Publisher";

        using var connection = await CreateConnectionAsync();
        return await connection.ExecuteScalarAsync<int>(sql);
    }

    public async Task<List<Publisher>> GetPageAsync(int pageNumber, int pageSize)
    {
        const string sql = @"
        SELECT p.PublisherID, p.PublisherName, p.CountryID, c.CountryID, c.CountryName
        FROM Publisher p
        LEFT JOIN Country c ON p.CountryID = c.CountryID
        ORDER BY p.PublisherName
        OFFSET @Offset ROWS
        FETCH NEXT @PageSize ROWS ONLY";

        using var connection = await CreateConnectionAsync();
        var publishers = await connection.QueryAsync<Publisher, Country, Publisher>(
            sql,
            (publisher, country) =>
            {
                publisher.Country = country;
                return publisher;
            },
            new
            {
                Offset = (pageNumber - 1) * pageSize,
                PageSize = pageSize
            },
            splitOn: "CountryID");

        return publishers.ToList();
    }

    public async Task<List<Publisher>> GetAllAsync()
    {
        const string sql = @"
        SELECT p.PublisherID, p.PublisherName, p.CountryID, c.CountryID, c.CountryName
        FROM Publisher p
        LEFT JOIN Country c ON p.CountryID = c.CountryID
        ORDER BY p.PublisherName";

        using var connection = await CreateConnectionAsync();
        var publishers = await connection.QueryAsync<Publisher, Country, Publisher>(
            sql,
            (publisher, country) =>
            {
                publisher.Country = country;
                return publisher;
            },
            splitOn: "CountryID");

        return publishers.ToList();
    }
}
