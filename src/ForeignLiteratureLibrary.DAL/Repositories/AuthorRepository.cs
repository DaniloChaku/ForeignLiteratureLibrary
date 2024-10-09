using Dapper;
using ForeignLiteratureLibrary.DAL.Entities;
using ForeignLiteratureLibrary.DAL.Interfaces;

namespace ForeignLiteratureLibrary.DAL.Repositories;

public class AuthorRepository : BaseRepository, IAuthorRepository
{
    public AuthorRepository(string connectionString) : base(connectionString)
    {
    }

    public async Task AddAsync(Author author)
    {
        const string sql = @"
                INSERT INTO Author (FullName, CountryCode)
                OUTPUT INSERTED.AuthorID
                VALUES (@FullName, @CountryCode)";

        using var connection = await CreateConnectionAsync();
        var authorId = await connection.ExecuteScalarAsync<int>(sql, author);
        author.AuthorID = authorId;
    }

    public async Task UpdateAsync(Author author)
    {
        const string sql = @"
                UPDATE Author 
                SET FullName = @FullName, CountryCode = @CountryCode
                WHERE AuthorID = @AuthorID";

        using var connection = await CreateConnectionAsync();
        await connection.ExecuteAsync(sql, author);
    }

    public async Task DeleteAsync(int authorId)
    {
        const string sql = @"
                DELETE FROM Author 
                WHERE AuthorID = @AuthorID";

        using var connection = await CreateConnectionAsync();
        await connection.ExecuteAsync(sql, new { AuthorID = authorId });
    }

    public async Task<Author?> GetByIdAsync(int authorId)
    {
        const string sql = @"
                SELECT a.AuthorID, a.FullName, a.CountryCode,
                       c.CountryCode, c.Name as CountryName,
                       b.BookID, b.OriginalTitle, b.OriginalLanguageCode, b.PublicationYear
                FROM Author a
                LEFT JOIN Country c ON a.CountryCode = c.CountryCode
                LEFT JOIN BookAuthor ba ON a.AuthorID = ba.AuthorID
                LEFT JOIN Book b ON ba.BookID = b.BookID
                WHERE a.AuthorID = @AuthorID";

        using var connection = await CreateConnectionAsync();

        Dictionary<int, Author> authorDict = [];

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
            splitOn: "CountryCode,BookID"
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
                SELECT a.AuthorID, a.FullName, a.CountryCode, c.CountryCode, c.Name as CountryName
                FROM Author a
                JOIN Country c ON a.CountryCode = c.CountryCode
                ORDER BY a.FullName
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
            splitOn: "CountryCode"
        );

        return authors.ToList();
    }

    public async Task<List<TopAuthor>> GetTop10AuthorsAsync(DateTime startDate, DateTime endDate)
    {
        const string sql = @"
            SELECT TOP 10 
            a.AuthorID, 
            a.FullName, 
            COUNT(bel.BookEditionLoanID) as LoanCount,
            c.Name as CountryName
            FROM Author a
            JOIN BookAuthor ba ON a.AuthorID = ba.AuthorID
            JOIN Book b ON ba.BookID = b.BookID
            JOIN BookEdition be ON b.BookID = be.BookID
            JOIN BookEditionLoan bel ON be.BookEditionID = bel.BookEditionID
            JOIN Country c ON a.CountryCode = c.CountryCode
            WHERE bel.LoanDate BETWEEN @StartDate AND @EndDate
            GROUP BY a.AuthorID, a.FullName, c.Name
            ORDER BY LoanCount DESC";

        using var connection = await CreateConnectionAsync();
        var topAuthors = await connection.QueryAsync<TopAuthor>(
            sql,
            new { StartDate = startDate, EndDate = endDate }
        );

        return topAuthors.ToList();
    }

}