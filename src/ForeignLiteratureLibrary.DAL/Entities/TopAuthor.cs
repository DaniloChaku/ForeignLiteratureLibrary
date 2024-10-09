namespace ForeignLiteratureLibrary.DAL.Entities;

public class TopAuthor
{
    public int AuthorId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public int LoanCount { get; set; }
}
