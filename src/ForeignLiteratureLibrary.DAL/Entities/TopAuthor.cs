namespace ForeignLiteratureLibrary.DAL.Entities;

public class TopAuthor
{
    public int AuthorId { get; set; }
    public string AuthorFullName { get; set; } = string.Empty;
    public int LoanCount { get; set; }
}
