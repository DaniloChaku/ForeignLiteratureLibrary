namespace ForeignLiteratureLibrary.DAL.Entities;

public class Publisher
{
    public int PublisherID { get; set; }
    public string PublisherName { get; set; } = string.Empty;
    public int CountryID { get; set; }
    public Country? Country { get; set; }
}
