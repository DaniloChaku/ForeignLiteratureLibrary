namespace ForeignLiteratureLibrary.DAL.Entities;

public class Author
{
    public int AuthorID { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string CountryOfOrigin { get; set; } = string.Empty;
    public Country? Country { get; set; }
}
