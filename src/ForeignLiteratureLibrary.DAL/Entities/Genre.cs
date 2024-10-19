namespace ForeignLiteratureLibrary.DAL.Entities;

public class Genre
{
    public int GenreID { get; set; }
    public string GenreName { get; set; } = string.Empty;

    public ICollection<Book> Books { get; set; } = [];
}
