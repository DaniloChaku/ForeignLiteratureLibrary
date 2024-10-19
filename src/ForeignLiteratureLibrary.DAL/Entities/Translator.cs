namespace ForeignLiteratureLibrary.DAL.Entities;

public class Translator
{
    public int TranslatorID { get; set; }
    public string TranslatorFullName { get; set; } = string.Empty;
    public int CountryID { get; set; }

    public Country? Country { get; set; }
    public ICollection<BookEdition> BookEditions { get; set; } = [];
}
