namespace ForeignLiteratureLibrary.DAL.Entities;

public class Loan
{
    public int LoanID { get; set; }
    public int BookEditionID { get; set; }
    public int ReaderID { get; set; }
    public DateTime LoanDate { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime? ReturnDate { get; set; }

    public BookEdition? BookEdition { get; set; }
    public Reader? Reader { get; set; }
}
