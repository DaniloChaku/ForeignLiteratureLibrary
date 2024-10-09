using Dapper;
using ForeignLiteratureLibrary.DAL.Entities;
using ForeignLiteratureLibrary.DAL.Interfaces;

namespace ForeignLiteratureLibrary.DAL.Repositories;

public class PublisherRepository : BaseRepository, IPublisherRepository
{
    public PublisherRepository(string connectionString) : base(connectionString)
    {
    }

    public async Task AddAsync(Publisher publisher)
    {
        const string sql = @"
                INSERT INTO Publisher (Name)
                OUTPUT INSERTED.PublisherID
                VALUES (@Name)";

        using var connection = await CreateConnectionAsync();
        var publisherId = await connection.ExecuteScalarAsync<int>(sql, publisher);
        publisher.PublisherID = publisherId;
    }

    public async Task UpdateAsync(Publisher publisher)
    {
        const string sql = @"
                UPDATE Publisher 
                SET Name = @Name
                WHERE PublisherID = @PublisherId";

        using var connection = await CreateConnectionAsync();
        await connection.ExecuteAsync(sql, publisher);
    }

    public async Task DeleteAsync(int publisherId)
    {
        const string sql = @"
                DELETE FROM Publisher 
                WHERE PublisherID = @PublisherId";

        using var connection = await CreateConnectionAsync();
        await connection.ExecuteAsync(sql, new { PublisherId = publisherId });
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
