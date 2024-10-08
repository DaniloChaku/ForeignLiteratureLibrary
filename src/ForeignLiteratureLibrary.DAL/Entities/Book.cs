namespace ForeignLiteratureLibrary.DAL.Entities;

public class Book
{
    public int BookId { get; set; }
    public string OriginalTitle { get; set; } = string.Empty;
    public string LanguageCode { get; set; } = string.Empty;
    public int PublicationYear { get; set; }

    public Language? Language { get; set; }
    public ICollection<Author> Authors { get; set; } = [];
    public ICollection<Genre> Genres { get; set; } = [];
    public ICollection<BookEdition> Editions { get; set; } = [];
}
