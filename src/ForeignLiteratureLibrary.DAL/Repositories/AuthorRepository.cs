using Dapper;
using ForeignLiteratureLibrary.DAL.Entities;
using ForeignLiteratureLibrary.DAL.Exceptions;
using ForeignLiteratureLibrary.DAL.Interfaces;
using Microsoft.Data.SqlClient;

namespace ForeignLiteratureLibrary.DAL.Repositories;

public class AuthorRepository : BaseRepository, IAuthorRepository
{
    public AuthorRepository(string connectionString) : base(connectionString)
    {
    }

    public async Task AddAsync(Author author)
    {
        try
        {
            const string sql = @"
            INSERT INTO Author (AuthorFullName, CountryID, BirthYear, DeathYear)
            OUTPUT INSERTED.AuthorID
            VALUES (@AuthorFullName, @CountryID, @BirthYear, @DeathYear)";

            using var connection = await CreateConnectionAsync();
            var authorId = await connection.ExecuteScalarAsync<int>(sql, author);
            author.AuthorID = authorId;
        }
        catch (SqlException ex) when (ex.Number == 547 && ex.Message.Contains("CHK_Author_AuthorFullName"))
        {
            throw new CheckConstraintViolationException(
                "Cannot add the author because the full name cannot be empty", ex);
        }
        catch (SqlException ex) when (ex.Number == 547 && ex.Message.Contains("FK_Author_CountryID"))
        {
            throw new ForeignKeyViolationException(
                $"Cannot add the author because the country '{author.CountryID}' does not exist", ex);
        }
        catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
        {
            throw new UniqueConstraintViolationException(
                "Cannot add the author because the data already exists", ex);
        }
        catch (SqlException ex) when (ex.Number == 515)
        {
            throw new NotNullConstraintViolationException(
                "Cannot add the author because a required field is missing", ex);
        }
    }

    public async Task UpdateAsync(Author author)
    {
        try
        {
            const string sql = @"
            UPDATE Author 
            SET AuthorFullName = @AuthorFullName, 
            CountryID = @CountryID,
            BirthYear = @BirthYear,
            DeathYear = @DeathYear
            WHERE AuthorID = @AuthorID";

            using var connection = await CreateConnectionAsync();
            await connection.ExecuteAsync(sql, author);
        }
        catch (SqlException ex) when (ex.Number == 547 && ex.Message.Contains("CHK_Author_AuthorFullName"))
        {
            throw new CheckConstraintViolationException(
                "Cannot update the author because the full name cannot be empty", ex);
        }
        catch (SqlException ex) when (ex.Number == 547 && ex.Message.Contains("FK_Author_CountryID"))
        {
            throw new ForeignKeyViolationException(
                $"Cannot update the author because the country '{author.CountryID}' does not exist", ex);
        }
        catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
        {
            throw new UniqueConstraintViolationException(
                "Cannot update the author because the data already exists", ex);
        }
        catch (SqlException ex) when (ex.Number == 515)
        {
            throw new NotNullConstraintViolationException(
                "Cannot update the author because a required field is missing", ex);
        }
    }

    public async Task DeleteAsync(int authorId)
    {
        try
        {
            const string sql = @"
            DELETE FROM Author 
            WHERE AuthorID = @AuthorID";

            using var connection = await CreateConnectionAsync();
            await connection.ExecuteAsync(sql, new { AuthorID = authorId });
        }
        catch (SqlException ex) when (ex.Number == 547)
        {
            throw new ForeignKeyViolationException(
                $"Cannot delete author '{authorId}' because it is referenced by other entities.", ex);
        }
    }

    public async Task<Author?> GetByIdAsync(int authorId)
    {
        const string sql = @"
            SELECT a.AuthorID, a.AuthorFullName, a.BirthYear, a.DeathYear, a.CountryID,
                   c.CountryID, c.CountryName,
                   b.BookID, b.OriginalTitle, b.FirstPublicationYear, b.BookDescription
            FROM Author a
            LEFT JOIN Country c ON a.CountryID = c.CountryID
            LEFT JOIN BookAuthor ba ON a.AuthorID = ba.AuthorID
            LEFT JOIN Book b ON ba.BookID = b.BookID
            WHERE a.AuthorID = @AuthorID";

        using var connection = await CreateConnectionAsync();

        var authorDict = new Dictionary<int, Author>();

        await connection.QueryAsync<Author, Country, Book, Author>(
            sql,
            (author, country, book) =>
            {
                if (!authorDict.TryGetValue(author.AuthorID, out var authorEntry))
                {
                    authorEntry = author;
                    authorEntry.Country = country;
                    authorEntry.Books = [];
                    authorDict.Add(author.AuthorID, authorEntry);
                }

                if (book != null)
                {
                    authorEntry.Books.Add(book);
                }

                return authorEntry;
            },
            new { AuthorID = authorId },
            splitOn: "CountryID, BookID"
        );

        return authorDict.Values.FirstOrDefault();
    }

    public async Task<int> GetCountAsync()
    {
        const string sql = "SELECT COUNT(*) FROM Author";

        using var connection = await CreateConnectionAsync();
        return await connection.ExecuteScalarAsync<int>(sql);
    }

    public async Task<List<Author>> GetPageAsync(int pageNumber, int pageSize)
    {
        const string sql = @"
            SELECT a.AuthorID, a.AuthorFullName, a.BirthYear, a.DeathYear, a.CountryID, 
            c.CountryID, c.CountryName
            FROM Author a
            JOIN Country c ON a.CountryID = c.CountryID
            ORDER BY a.AuthorFullName
            OFFSET @Offset ROWS
            FETCH NEXT @PageSize ROWS ONLY";

        using var connection = await CreateConnectionAsync();
        var authors = await connection.QueryAsync<Author, Country, Author>(
            sql,
            (author, country) =>
            {
                author.Country = country;
                return author;
            },
            new { Offset = (pageNumber - 1) * pageSize, PageSize = pageSize },
            splitOn: "CountryID"
        );

        return authors.ToList();
    }

    public async Task<List<TopAuthor>> GetTop10AuthorsAsync(DateTime startDate, DateTime endDate)
    {
        const string sql = @"
        SELECT TOP 10 
        a.AuthorID, 
        a.AuthorFullName, 
        COUNT(l.LoanID) as LoanCount,
        c.CountryName
        FROM Author a
        JOIN BookAuthor ba ON a.AuthorID = ba.AuthorID
        JOIN Book b ON ba.BookID = b.BookID
        JOIN BookEdition be ON b.BookID = be.BookID
        JOIN Loan l ON be.BookEditionID = l.BookEditionID
        JOIN Country c ON a.CountryID = c.CountryID
        WHERE l.LoanDate BETWEEN @StartDate AND @EndDate
        GROUP BY a.AuthorID, a.AuthorFullName, c.CountryName
        ORDER BY LoanCount DESC";

        using var connection = await CreateConnectionAsync();
        var topAuthors = await connection.QueryAsync<TopAuthor>(
            sql,
            new { StartDate = startDate, EndDate = endDate }
        );

        return topAuthors.ToList();
    }

    public async Task<List<Author>> GetAllAsync()
    {
        const string sql = @"
            SELECT AuthorID, AuthorFullName, CountryID
            FROM Author
            ORDER BY AuthorFullName";

        using var connection = await CreateConnectionAsync();
        var authors = await connection.QueryAsync<Author>(sql);

        return authors.ToList();
    }
}