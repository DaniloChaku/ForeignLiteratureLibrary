namespace ForeignLiteratureLibrary.DAL.Entities;

public class BookEdition
{
    public string ISBN { get; set; } = string.Empty;
    public int BookID { get; set; }
    public string TranslatedTitle { get; set; } = string.Empty;
    public int PublicationYear { get; set; }
    public string LanguageCode { get; set; } = string.Empty;
    public int PageCount { get; set; }
    public string ShelfLocation { get; set; } = string.Empty;
    public int AvailableCopies { get; set; }
    public int PublisherID { get; set; }
    public Book? Book { get; set; }
    public Language? Language { get; set; }
    public Publisher? Publisher { get; set; }
}
