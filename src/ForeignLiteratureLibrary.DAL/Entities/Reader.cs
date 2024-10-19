namespace ForeignLiteratureLibrary.DAL.Entities;

public class Reader
{
    public int ReaderID { get; set; }
    public string LibraryCardNumber { get; set; } = string.Empty;
    public string ReaderFullName { get; set; } = string.Empty;
    public string? EmailAddress { get; set; }
    public string? PhoneNumber { get; set; }

    public ICollection<Loan> Loans { get; set; } = [];
}
