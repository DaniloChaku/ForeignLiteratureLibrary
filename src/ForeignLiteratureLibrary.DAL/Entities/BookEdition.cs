namespace ForeignLiteratureLibrary.DAL.Entities;

public class BookEdition
{
    public int BookEditionID { get; set; }
    public string? ISBN { get; set; }
    public string EditionTitle { get; set; } = string.Empty;
    public int PageCount { get; set; }
    public string ShelfLocation { get; set; } = string.Empty;
    public int TotalCopies { get; set; }
    public int EditionPublicationYear { get; set;}
    public int LanguageID { get; set; }
    public int BookID { get; set; }
    public int? PublisherID { get; set; }

    public Book? Book { get; set; }
    public Language? Language { get; set; }
    public Publisher? Publisher { get; set; }
    public ICollection<Translator> Translators { get; set; } = [];
    public ICollection<Loan> Loans { get; set; } = [];

    public int AvailableCopies { get; set; }
}
