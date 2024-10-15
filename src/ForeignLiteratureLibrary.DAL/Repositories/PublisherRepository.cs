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
            INSERT INTO Publisher (Name)
            OUTPUT INSERTED.PublisherID
            VALUES (@Name)";

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
            if (ex.Message.Contains("CHK_Publisher_Name", StringComparison.InvariantCultureIgnoreCase))
            {
                throw new CheckConstraintViolationException(
                    "Cannot add the publisher because its Name should be at least 1 character long", ex);
            }
            throw;
        }
        catch (SqlException ex) when (ex.Number == 515)
        {
            throw new NotNullConstraintViolationException(
                "Cannot add the publisher because its name is not provided", ex);
        }
    }

    public async Task UpdateAsync(Publisher publisher)
    {
        try
        {
            const string sql = @"
            UPDATE Publisher 
            SET Name = @Name
            WHERE PublisherID = @PublisherId";

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
            if (ex.Message.Contains("CHK_Publisher_Name", StringComparison.InvariantCultureIgnoreCase))
            {
                throw new CheckConstraintViolationException(
                    "Cannot update the publisher because its Name should be at least 1 character long", ex);
            }
            throw;
        }
        catch (SqlException ex) when (ex.Number == 515)
        {
            throw new NotNullConstraintViolationException(
                "Cannot update the publisher because its name is not provided", ex);
        }
    }
    public async Task DeleteAsync(int publisherId)
    {
        try
        {
            const string sql = @"
            DELETE FROM Publisher 
            WHERE PublisherID = @PublisherId";

            using var connection = await CreateConnectionAsync();
            await connection.ExecuteAsync(sql, new { PublisherId = publisherId });
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
                SELECT PublisherID, Name 
                FROM Publisher 
                WHERE PublisherID = @PublisherId";

        using var connection = await CreateConnectionAsync();
        return await connection.QuerySingleOrDefaultAsync<Publisher>(sql, new { PublisherId = publisherId });
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
                SELECT PublisherID, Name
                FROM Publisher
                ORDER BY Name
                OFFSET @Offset ROWS
                FETCH NEXT @PageSize ROWS ONLY";

        using var connection = await CreateConnectionAsync();
        var publishers = await connection.QueryAsync<Publisher>(sql,
            new
            {
                Offset = (pageNumber - 1) * pageSize,
                PageSize = pageSize
            });

        return publishers.ToList();
    }

    public async Task<List<Publisher>> GetAllAsync()
    {
        const string sql = @"
                SELECT PublisherID, Name
                FROM Publisher
                ORDER BY Name";

        using var connection = await CreateConnectionAsync();
        var publishers = await connection.QueryAsync<Publisher>(sql);
        return publishers.ToList();
    }
}
