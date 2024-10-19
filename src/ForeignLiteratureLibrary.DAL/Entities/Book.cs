namespace ForeignLiteratureLibrary.DAL.Entities;

public class Book
{
    public int BookID { get; set; }
    public string OriginalTitle { get; set; } = string.Empty;
    public string? BookDescription { get; set; } = string.Empty;
    public int FirstPublicationYear { get; set; }
    public int OriginalLanguageID { get; set; }

    public Language? OriginalLanguage { get; set; }
    public ICollection<Author> Authors { get; set; } = [];
    public ICollection<Genre> Genres { get; set; } = [];
    public ICollection<BookEdition> Editions { get; set; } = [];
}
