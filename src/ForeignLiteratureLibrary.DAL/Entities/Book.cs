namespace ForeignLiteratureLibrary.DAL.Entities;

public class Book
{
    public int BookID { get; set; }
    public string OriginalTitle { get; set; } = string.Empty;
    public string OriginalLanguageCode { get; set; } = string.Empty;
    public Language? Language { get; set; }
}
