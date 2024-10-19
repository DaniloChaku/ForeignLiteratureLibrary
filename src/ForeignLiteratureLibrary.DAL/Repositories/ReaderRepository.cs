using Dapper;
using ForeignLiteratureLibrary.DAL.Entities;
using ForeignLiteratureLibrary.DAL.Exceptions;
using ForeignLiteratureLibrary.DAL.Interfaces;
using Microsoft.Data.SqlClient;

namespace ForeignLiteratureLibrary.DAL.Repositories;

public class ReaderRepository : BaseRepository, IReaderRepository
{
    public ReaderRepository(string connectionString) : base(connectionString)
    {
    }

    public async Task AddAsync(Reader reader)
    {
        try
        {
            const string sql = @"
            INSERT INTO Reader (LibraryCardNumber, ReaderFullName, EmailAddress, PhoneNumber)
            VALUES (@LibraryCardNumber, @ReaderFullName, @EmailAddress, @PhoneNumber)";

            using var connection = await CreateConnectionAsync();
            await connection.ExecuteAsync(sql, reader);
        }
        catch (SqlException ex) when (ex.Number == 547 && ex.Message.Contains("CHK_Reader_ReaderFullName"))
        {
            throw new CheckConstraintViolationException(
                "Cannot add the reader because the full name cannot be empty", ex);
        }
        catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
        {
            throw new UniqueConstraintViolationException(
                $"Cannot add the reader because a reader with the library card number '{reader.LibraryCardNumber}' already exists", ex);
        }
        catch (SqlException ex) when (ex.Number == 515)
        {
            throw new NotNullConstraintViolationException(
                "Cannot add the reader because a required field is missing", ex);
        }
    }

    public async Task UpdateAsync(Reader reader)
    {
        try
        {
            const string sql = @"
            UPDATE Reader 
            SET LibraryCardNumber = @LibraryCardNumber,
                ReaderFullName = @ReaderFullName,
                EmailAddress = @EmailAddress,
                PhoneNumber = @PhoneNumber
            WHERE ReaderID = @ReaderID";

            using var connection = await CreateConnectionAsync();
            await connection.ExecuteAsync(sql, reader);
        }
        catch (SqlException ex) when (ex.Number == 547 && ex.Message.Contains("CHK_Reader_ReaderFullName"))
        {
            throw new CheckConstraintViolationException(
                "Cannot update the reader because the full name cannot be empty", ex);
        }
        catch (SqlException ex) when (ex.Number == 515)
        {
            throw new NotNullConstraintViolationException(
                "Cannot update the reader because a required field is missing", ex);
        }
    }

    public async Task DeleteAsync(int readerId)
    {
        try
        {
            const string sql = @"
                DELETE FROM Reader 
                WHERE ReaderID = @ReaderID";

            using var connection = await CreateConnectionAsync();
            await connection.ExecuteAsync(sql, new { ReaderID = readerId });
        }
        catch (SqlException ex) when (ex.Number == 547)
        {
            throw new ForeignKeyViolationException(
                $"Cannot delete the reader '{readerId}' because they are referenced by other entities.", ex);
        }
    }

    public async Task<Reader?> GetByIdAsync(int readerId)
    {
        const string sql = @"
        SELECT r.*, l.*, be.*
        FROM Reader r
        LEFT JOIN Loan l ON r.ReaderID = l.ReaderID
        LEFT JOIN BookEdition be ON l.BookEditionID = be.BookEditionID
        WHERE r.ReaderID = @ReaderID";

        using var connection = await CreateConnectionAsync();
        var readerDictionary = new Dictionary<int, Reader>();

        await connection.QueryAsync<Reader, Loan, BookEdition, Reader>(
            sql,
            (reader, loan, bookEdition) =>
            {
                if (!readerDictionary.TryGetValue(reader.ReaderID, out var readerEntry))
                {
                    readerEntry = reader;
                    readerEntry.Loans = new List<Loan>();
                    readerDictionary.Add(reader.ReaderID, readerEntry);
                }
                if (loan != null)
                {
                    loan.BookEdition = bookEdition;
                    readerEntry.Loans.Add(loan);
                }
                return readerEntry;
            },
            new { ReaderID = readerId },
            splitOn: "LoanID,BookEditionID");

        return readerDictionary.Values.FirstOrDefault();
    }

    public async Task<List<Reader>> GetAllAsync()
    {
        const string sql = "SELECT * FROM Reader";

        using var connection = await CreateConnectionAsync();
        var readers = await connection.QueryAsync<Reader>(sql);
        return readers.ToList();
    }


    public async Task<int> GetCountAsync()
    {
        const string sql = "SELECT COUNT(*) FROM Reader";

        using var connection = await CreateConnectionAsync();
        return await connection.ExecuteScalarAsync<int>(sql);
    }

    public async Task<List<Reader>> GetByFullNameAsync(string readerFullName)
    {
        const string sql = @"
                SELECT ReaderID, LibraryCardNumber, ReaderFullName, EmailAddress, PhoneNumber
                FROM Reader
                WHERE ReaderFullName LIKE @ReaderFullName";

        using var connection = await CreateConnectionAsync();
        var readers = await connection.QueryAsync<Reader>(sql, new { ReaderFullName = $"%{readerFullName}%" });
        return readers.ToList();
    }

    public async Task<List<Reader>> GetPageAsync(int pageNumber, int pageSize)
    {
        const string sql = @"
                SELECT ReaderID, LibraryCardNumber, ReaderFullName, EmailAddress, PhoneNumber
                FROM Reader
                ORDER BY ReaderFullName
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

    public async Task<List<Reader>> GetByLibraryCardNumberAsync(string libraryCardNumber)
    {
        const string sql = @"
                SELECT ReaderID, LibraryCardNumber, ReaderFullName, EmailAddress, PhoneNumber
                FROM Reader
                WHERE LibraryCardNumber LIKE @LibraryCardNumber";

        using var connection = await CreateConnectionAsync();
        var readers = await connection.QueryAsync<Reader>(sql, new { libraryCardNumber = $"%{libraryCardNumber}%" });
        return readers.ToList();
    }
}
