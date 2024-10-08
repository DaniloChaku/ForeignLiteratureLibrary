namespace ForeignLiteratureLibrary.DAL.Entities;

public class BookEditionLoan
{
    public int LoanID { get; set; }
    public string ISBN { get; set; } = string.Empty;
    public string LibraryCardNumber { get; set; } = string.Empty;
    public DateTime LoanDate { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime? ReturnDate { get; set; }

    public BookEdition? BookEdition { get; set; }
    public Reader? Reader { get; set; }
}
