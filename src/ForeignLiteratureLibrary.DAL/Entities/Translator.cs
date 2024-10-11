namespace ForeignLiteratureLibrary.DAL.Entities;

public class Translator
{
    public int TranslatorID { get; set; }
    public string FullName { get; set; } = string.Empty;

    public ICollection<BookEdition> BookEditions { get; set; } = [];
}
