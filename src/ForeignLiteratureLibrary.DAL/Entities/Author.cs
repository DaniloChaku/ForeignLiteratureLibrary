namespace ForeignLiteratureLibrary.DAL.Entities;

public class Author
{
    public int AuthorID { get; set; }
    public string AuthorFullName { get; set; } = string.Empty;
    public int? BirthYear { get; set; }
    public int? DeathYear { get; set; }
    public int CountryID { get; set; }

    public Country? Country { get; set; }
    public ICollection<Book> Books { get; set; } = [];
}
