using Dapper;
using ForeignLiteratureLibrary.DAL.Entities;
using ForeignLiteratureLibrary.DAL.Interfaces;

namespace ForeignLiteratureLibrary.DAL.Repositories;

public class GenreRepository : BaseRepository, IGenreRepository
{
    public GenreRepository(string connectionString) : base(connectionString)
    {
    }

    public async Task AddAsync(Genre genre)
    {
        const string sql = @"
                INSERT INTO Genre (Name)
                OUTPUT INSERTED.GenreID
                VALUES (@Name)";

        using var connection = await CreateConnectionAsync();
        var genreId = await connection.ExecuteScalarAsync<int>(sql, genre);
        genre.GenreID = genreId;
    }

    public async Task UpdateAsync(Genre genre)
    {
        const string sql = @"
                UPDATE Genre 
                SET Name = @Name
                WHERE GenreID = @GenreId";

        using var connection = await CreateConnectionAsync();
        await connection.ExecuteAsync(sql, genre);
    }

    public async Task DeleteAsync(int genreId)
    {
        const string sql = @"
                DELETE FROM Genre 
                WHERE GenreID = @GenreId";

        using var connection = await CreateConnectionAsync();
        await connection.ExecuteAsync(sql, new { GenreId = genreId });
    }

    public async Task<Genre?> GetByIdAsync(int genreId)
    {
        const string sql = @"
                SELECT GenreID, Name 
                FROM Genre 
                WHERE GenreID = @GenreId";

        using var connection = await CreateConnectionAsync();
        return await connection.QuerySingleOrDefaultAsync<Genre>(sql, new { GenreId = genreId });
    }

    public async Task<int> GetCountAsync()
    {
        const string sql = "SELECT COUNT(*) FROM Genre";

        using var connection = await CreateConnectionAsync();
        return await connection.ExecuteScalarAsync<int>(sql);
    }

    public async Task<List<Genre>> GetPageAsync(int pageNumber, int pageSize)
    {
        const string sql = @"
                SELECT GenreID, Name
                FROM Genre
                ORDER BY Name
                OFFSET @Offset ROWS
                FETCH NEXT @PageSize ROWS ONLY";

        using var connection = await CreateConnectionAsync();
        var genres = await connection.QueryAsync<Genre>(sql,
            new
            {
                Offset = (pageNumber - 1) * pageSize,
                PageSize = pageSize
            });

        return genres.ToList();
    }

    public async Task<List<Genre>> GetAllAsync()
    {
        const string sql = @"
                SELECT GenreID, Name
                FROM Genre
                ORDER BY Name";

        using var connection = await CreateConnectionAsync();
        var genres = await connection.QueryAsync<Genre>(sql);
        return genres.ToList();
    }
}
