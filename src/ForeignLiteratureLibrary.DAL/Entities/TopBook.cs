namespace ForeignLiteratureLibrary.DAL.Entities;

public class TopBook
{
    public int BookId { get; set; }
    public string OriginalTitle { get; set; } = string.Empty;
    public int LoanCount { get; set; }
}
