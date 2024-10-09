using Dapper;
using ForeignLiteratureLibrary.DAL.Entities;
using ForeignLiteratureLibrary.DAL.Interfaces;

namespace ForeignLiteratureLibrary.DAL.Repositories;

public class ReaderRepository : BaseRepository, IReaderRepository
{
    public ReaderRepository(string connectionString) : base(connectionString)
    {
    }

    public async Task AddAsync(Reader reader)
    {
        const string sql = @"
                INSERT INTO Reader (LibraryCardNumber, FullName, DateOfBirth, Email, Phone, RegistrationDate)
                VALUES (@LibraryCardNumber, @FullName, @DateOfBirth, @Email, @Phone, @RegistrationDate)";

        using var connection = await CreateConnectionAsync();
        await connection.ExecuteAsync(sql, reader);
    }

    public async Task UpdateAsync(Reader reader)
    {
        const string sql = @"
                UPDATE Reader 
                SET FullName = @FullName,
                    DateOfBirth = @DateOfBirth,
                    Email = @Email,
                    Phone = @Phone
                WHERE LibraryCardNumber = @LibraryCardNumber";

        using var connection = await CreateConnectionAsync();
        await connection.ExecuteAsync(sql, reader);
    }

    public async Task DeleteAsync(string libraryCardNumber)
    {
        const string sql = @"
                DELETE FROM Reader 
                WHERE LibraryCardNumber = @LibraryCardNumber";

        using var connection = await CreateConnectionAsync();
        await connection.ExecuteAsync(sql, new { LibraryCardNumber = libraryCardNumber });
    }

    public async Task<Reader?> GetByLibraryCardNumberAsync(string libraryCardNumber)
    {
        const string sql = @"
                SELECT r.*, bel.*
                FROM Reader r
                LEFT JOIN BookEditionLoan bel ON r.LibraryCardNumber = bel.LibraryCardNumber
                WHERE r.LibraryCardNumber = @LibraryCardNumber
                    AND (bel.ReturnDate IS NULL OR bel.ReturnDate IS NOT NULL)";

        using var connection = await CreateConnectionAsync();

        var readerDictionary = new Dictionary<string, Reader>();

        await connection.QueryAsync<Reader, BookEditionLoan, Reader>(
            sql,
            (reader, loan) =>
            {
                if (!readerDictionary.TryGetValue(reader.LibraryCardNumber, out var readerEntry))
                {
                    readerEntry = reader;
                    readerEntry.Loans = new List<BookEditionLoan>();
                    readerDictionary.Add(reader.LibraryCardNumber, readerEntry);
                }

                if (loan != null)
                {
                    readerEntry.Loans.Add(loan);
                }

                return readerEntry;
            },
            new { LibraryCardNumber = libraryCardNumber },
            splitOn: "BookEditionLoanID");

        return readerDictionary.Values.FirstOrDefault();
    }

    public async Task<int> GetCountAsync()
    {
        const string sql = "SELECT COUNT(*) FROM Reader";

        using var connection = await CreateConnectionAsync();
        return await connection.ExecuteScalarAsync<int>(sql);
    }

    public async Task<List<Reader>> GetByFullNameAsync(string fullName)
    {
        const string sql = @"
                SELECT * FROM Reader 
                WHERE FullName LIKE @FullName";

        using var connection = await CreateConnectionAsync();
        var readers = await connection.QueryAsync<Reader>(sql, new { FullName = $"%{fullName}%" });
        return readers.ToList();
    }

    public async Task<List<Reader>> GetPageAsync(int pageNumber, int pageSize)
    {
        const string sql = @"
                SELECT *
                FROM Reader
                ORDER BY FullName
                OFFSET @Offset ROWS
                FETCH NEXT @PageSize ROWS ONLY";

        using var connection = await CreateConnectionAsync();
        var readers = await connection.QueryAsync<Reader>(sql,
            new
            {
                Offset = (pageNumber - 1) * pageSize,
                PageSize = pageSize
            });

        return readers.ToList();
    }
}
