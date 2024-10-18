using ForeignLiteratureLibrary.BLL.Dtos;

namespace ForeignLiteratureLibrary.Web.Models;

public class BookWithGenresAndAuthorsViewModel
{
    public BookDto Book { get; set; } = new();
    public List<int> Authors { get; set; } = [];
    public List<int> Genres { get; set; } = [];

    public void PopulateBookCollections()
    {
        Book.Authors.AddRange(Authors.Select(id => new AuthorDto { AuthorID = id }));

        Book.Genres.AddRange(Genres.Select(id => new GenreDto { GenreID = id }));
    }
}
