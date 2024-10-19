using Dapper;
using ForeignLiteratureLibrary.DAL.Entities;
using ForeignLiteratureLibrary.DAL.Exceptions;
using ForeignLiteratureLibrary.DAL.Interfaces;
using Microsoft.Data.SqlClient;

namespace ForeignLiteratureLibrary.DAL.Repositories;

public class GenreRepository : BaseRepository, IGenreRepository
{
    public GenreRepository(string connectionString) : base(connectionString)
    {
    }

    public async Task AddAsync(Genre genre)
    {
        try
        {
            const string sql = @"
            INSERT INTO Genre (GenreName)
            OUTPUT INSERTED.GenreID
            VALUES (@GenreName)";

            using var connection = await CreateConnectionAsync();
            var genreId = await connection.ExecuteScalarAsync<int>(sql, genre);
            genre.GenreID = genreId;
        }
        catch (SqlException ex) when (ex.Number == 547 && ex.Message.Contains("CHK_GenreName"))
        {
            throw new CheckConstraintViolationException(
                "Cannot add the genre because the name cannot be empty", ex);
        }
        catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
        {
            throw new UniqueConstraintViolationException(
                $"Cannot add the genre because the name '{genre.GenreName}' already exists", ex);
        }
        catch (SqlException ex) when (ex.Number == 515)
        {
            throw new NotNullConstraintViolationException(
                "Cannot add the genre because a required field is missing", ex);
        }
    }

    public async Task UpdateAsync(Genre genre)
    {
        try
        {
            const string sql = @"
            UPDATE Genre 
            SET GenreName = @GenreName
            WHERE GenreID = @GenreID";

            using var connection = await CreateConnectionAsync();
            await connection.ExecuteAsync(sql, genre);
        }
        catch (SqlException ex) when (ex.Number == 547 && ex.Message.Contains("CHK_GenreName"))
        {
            throw new CheckConstraintViolationException(
                "Cannot update the genre because the name cannot be empty", ex);
        }
        catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
        {
            throw new UniqueConstraintViolationException(
                $"Cannot update the genre because the name '{genre.GenreName}' already exists", ex);
        }
        catch (SqlException ex) when (ex.Number == 515)
        {
            throw new NotNullConstraintViolationException(
                "Cannot update the genre because a required field is missing", ex);
        }
    }

    public async Task DeleteAsync(int genreId)
    {
        try
        {
            const string sql = @"
            DELETE FROM Genre 
            WHERE GenreID = @GenreID";

            using var connection = await CreateConnectionAsync();
            await connection.ExecuteAsync(sql, new { GenreID = genreId });
        }
        catch (SqlException ex) when (ex.Number == 547)
        {
            throw new ForeignKeyViolationException(
                $"Cannot delete genre '{genreId}' because it is referenced by other entities.", ex);
        }
    }

    public async Task<Genre?> GetByIdAsync(int genreId)
    {
        const string sql = @"
        SELECT g.GenreID, g.GenreName, b.BookID, b.OriginalTitle
        FROM Genre g
        LEFT JOIN BookGenre bg ON g.GenreID = bg.GenreID
        LEFT JOIN Book b ON bg.BookID = b.BookID
        WHERE g.GenreID = @GenreID";

        using var connection = await CreateConnectionAsync();
        var genreWithBooks = await connection.QueryAsync<Genre, Book, Genre>(
            sql,
            (genre, book) =>
            {
                if (book != null)
                {
                    genre.Books.Add(book);
                }
                return genre;
            },
            new { GenreID = genreId },
            splitOn: "BookID");

        return genreWithBooks.FirstOrDefault();
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
            SELECT GenreID, GenreName
            FROM Genre
            ORDER BY GenreName
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
            SELECT GenreID, GenreName
            FROM Genre
            ORDER BY GenreName";

        using var connection = await CreateConnectionAsync();
        var genres = await connection.QueryAsync<Genre>(sql);
        return genres.ToList();
    }
}
