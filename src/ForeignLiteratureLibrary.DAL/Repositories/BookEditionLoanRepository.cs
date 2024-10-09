using Dapper;
using ForeignLiteratureLibrary.DAL.Entities;
using ForeignLiteratureLibrary.DAL.Interfaces;

namespace ForeignLiteratureLibrary.DAL.Repositories;

public class BookEditionLoanRepository : BaseRepository, IBookEditionLoanRepository
{
    public BookEditionLoanRepository(string connectionString) : base(connectionString)
    {
    }

    private const string BaseSelectQuery = @"
            SELECT 
                bel.*, be.*, r.*
            FROM BookEditionLoan bel
            JOIN BookEdition be ON bel.BookEditionID = be.BookEditionID
            JOIN Reader r ON bel.LibraryCardNumber = r.LibraryCardNumber";

    public async Task AddAsync(BookEditionLoan loan)
    {
        const string sql = @"
                INSERT INTO BookEditionLoan 
                (BookEditionID, LibraryCardNumber, LoanDate, DueDate, ReturnDate)
                VALUES 
                (@BookEditionID, @LibraryCardNumber, @LoanDate, @DueDate, @ReturnDate);
                SELECT CAST(SCOPE_IDENTITY() as int)";

        using var connection = await CreateConnectionAsync();
        loan.BookEditionLoanID = await connection.QuerySingleAsync<int>(sql, loan);
    }

    public async Task UpdateAsync(BookEditionLoan loan)
    {
        const string sql = @"
                UPDATE BookEditionLoan 
                SET BookEditionID = @BookEditionID,
                    LibraryCardNumber = @LibraryCardNumber,
                    LoanDate = @LoanDate,
                    DueDate = @DueDate,
                    ReturnDate = @ReturnDate
                WHERE BookEditionLoanID = @BookEditionLoanID";

        using var connection = await CreateConnectionAsync();
        await connection.ExecuteAsync(sql, loan);
    }

    public async Task DeleteAsync(int loanId)
    {
        const string sql = "DELETE FROM BookEditionLoan WHERE BookEditionLoanID = @LoanId";

        using var connection = await CreateConnectionAsync();
        await connection.ExecuteAsync(sql, new { LoanId = loanId });
    }

    public async Task<BookEditionLoan?> GetByIdAsync(int loanId)
    {
        var sql = $"{BaseSelectQuery} WHERE bel.BookEditionLoanID = @LoanId";

        using var connection = await CreateConnectionAsync();
        var loans = await connection.QueryAsync<BookEditionLoan, BookEdition, Reader, BookEditionLoan>(
            sql,
            (loan, bookEdition, reader) =>
            {
                loan.BookEdition = bookEdition;
                loan.Reader = reader;
                return loan;
            },
            new { LoanId = loanId },
            splitOn: "ISBN,LibraryCardNumber"
        );

        return loans.FirstOrDefault();
    }

    public async Task<int> GetCountAsync()
    {
        const string sql = "SELECT COUNT(*) FROM BookEditionLoan";

        using var connection = await CreateConnectionAsync();
        return await connection.ExecuteScalarAsync<int>(sql);
    }

    public async Task<List<BookEditionLoan>> GetPageAsync(int pageNumber, int pageSize)
    {
        var sql = $@"{BaseSelectQuery}
                ORDER BY bel.LoanDate DESC
                OFFSET @Offset ROWS
                FETCH NEXT @PageSize ROWS ONLY";

        using var connection = await CreateConnectionAsync();
        var loans = await connection.QueryAsync<BookEditionLoan, BookEdition, Reader, BookEditionLoan>(
            sql,
            (loan, bookEdition, reader) =>
            {
                loan.BookEdition = bookEdition;
                loan.Reader = reader;
                return loan;
            },
            new
            {
                Offset = (pageNumber - 1) * pageSize,
                PageSize = pageSize
            },
            splitOn: "ISBN,LibraryCardNumber"
        );

        return loans.ToList();
    }

    public async Task<List<BookEditionLoan>> GetOverduePageAsync(int pageNumber, int pageSize)
    {
        var sql = $@"{BaseSelectQuery}
                WHERE bel.ReturnDate IS NULL AND bel.DueDate < GETDATE()
                ORDER BY bel.DueDate
                OFFSET @Offset ROWS
                FETCH NEXT @PageSize ROWS ONLY";

        using var connection = await CreateConnectionAsync();
        var loans = await connection.QueryAsync<BookEditionLoan, BookEdition, Reader, BookEditionLoan>(
            sql,
            (loan, bookEdition, reader) =>
            {
                loan.BookEdition = bookEdition;
                loan.Reader = reader;
                return loan;
            },
            new
            {
                Offset = (pageNumber - 1) * pageSize,
                PageSize = pageSize
            },
            splitOn: "ISBN,LibraryCardNumber"
        );

        return loans.ToList();
    }
}
