namespace ForeignLiteratureLibrary.DAL.Entities;

public class Reader
{
    public string LibraryCardNumber { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public DateTime RegistrationDate { get; set; }

    public ICollection<BookEditionLoan> Loans { get; set; } = [];
}
