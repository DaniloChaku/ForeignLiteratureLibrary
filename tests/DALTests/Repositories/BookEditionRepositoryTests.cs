using DALTests.TestHelpers;
using FluentAssertions;
using ForeignLiteratureLibrary.DAL.Entities;
using ForeignLiteratureLibrary.DAL.Repositories;
using Microsoft.Data.SqlClient;

namespace DALTests.Repositories;

public class BookEditionRepositoryTests : IDisposable
{
    private readonly BookEditionRepository _repository;
    private readonly string _connectionString = TestConnectionStringHelper.ConnectionString;
    private bool _disposed;

    public BookEditionRepositoryTests()
    {
        _repository = new BookEditionRepository(_connectionString);
        DbSeedHelper.SeedDb(_connectionString);
    }

    #region AddAsync

    [Fact]
    public async Task AddAsync_AddsNewBookEditionWithTranslators()
    {
        // Arrange
        var newBookEdition = new BookEdition
        {
            ISBN = "978-3-16-148410-0",
            Title = "New Book Edition",
            LanguageCode = "EN",
            PageCount = 350,
            ShelfLocation = "B1-01",
            TotalCopies = 5,
            AvailableCopies = 5,
            BookID = 1,
            PublisherID = 1, 
            Translators =
            [
                new Translator { TranslatorID = 1 },
                new Translator { TranslatorID = 2 },
            ]
        };

        // Act
        await _repository.AddAsync(newBookEdition);

        // Assert
        newBookEdition.BookEditionID.Should().BeGreaterThan(0);
        var result = await _repository.GetByIdAsync(newBookEdition.BookEditionID);
        result.Should().NotBeNull();
        result!.Title.Should().Be("New Book Edition");
        result.Translators.Should().Contain(t => t.TranslatorID == 1);
        result.Translators.Should().Contain(t => t.TranslatorID == 2);
    }

    [Fact]
    public async Task AddAsync_InvalidBookEdition_ThrowsSqlException()
    {
        // Arrange
        var invalidBookEdition = new BookEdition
        {
            ISBN = "",
            Title = "Invalid Book Edition",
            LanguageCode = "EN",
            PageCount = 350,
            ShelfLocation = "B1-01",
            TotalCopies = 5,
            AvailableCopies = 5,
            BookID = 1,
            PublisherID = 1
        };

        // Act & Assert
        await Assert.ThrowsAsync<SqlException>(() => _repository.AddAsync(invalidBookEdition));
    }

    #endregion

    #region UpdateAsync

    [Fact]
    public async Task UpdateAsync_UpdatesExistingBookEdition()
    {
        // Arrange
        var bookEdition = await _repository.GetByIdAsync(1);
        bookEdition!.Title = "Updated Book Edition";

        // Act
        await _repository.UpdateAsync(bookEdition);
        var updatedBookEdition = await _repository.GetByIdAsync(1);

        // Assert
        updatedBookEdition.Should().NotBeNull();
        updatedBookEdition!.Title.Should().Be("Updated Book Edition");
    }

    [Fact]
    public async Task UpdateAsync_UpdatesTranslators()
    {
        // Arrange
        var bookEdition = await _repository.GetByIdAsync(1); 
        bookEdition!.Translators =
        [
            new Translator { TranslatorID = 2 },
            new Translator { TranslatorID = 3 }  
        ];

        // Act
        await _repository.UpdateAsync(bookEdition);
        var updatedBookEdition = await _repository.GetByIdAsync(1);

        // Assert
        updatedBookEdition.Should().NotBeNull();
        updatedBookEdition!.Translators.Should().HaveCount(2);
        updatedBookEdition.Translators.Should().Contain(t => t.TranslatorID == 2);
        updatedBookEdition.Translators.Should().Contain(t => t.TranslatorID == 3);
    }

    #endregion

    #region DeleteAsync

    [Fact]
    public async Task DeleteAsync_DeletesBookEdition()
    {
        // Arrange
        var id = 4;

        // Act
        await _repository.DeleteAsync(id);
        var deletedBookEdition = await _repository.GetByIdAsync(4);

        // Assert
        deletedBookEdition.Should().BeNull();
    }

    #endregion

    #region GetCountAsync

    [Fact]
    public async Task GetCountAsync_ReturnsCorrectCountOfBookEditions()
    {
        // Act
        var count = await _repository.GetCountAsync();

        // Assert
        count.Should().Be(4);
    }

    #endregion

    #region GetByIdAsync

    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsCorrectBookEdition()
    {
        // Act
        var result = await _repository.GetByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.BookEditionID.Should().Be(1);
        result.ISBN.Should().Be("978-0-14-023750-4");
        result.Book!.OriginalTitle.Should().Be("The Catcher in the Rye");
        result.Publisher!.Name.Should().Be("Penguin Books");
        result.Language!.Name.Should().Be("English");
        result.Translators.Should().BeEmpty(); 
        result.Book.Authors.Should().ContainSingle(a => a.FullName == "J.D. Salinger");
        result.Book.Genres.Should().ContainSingle(g => g.Name == "Fiction");
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingId_ReturnsNull()
    {
        // Act
        var result = await _repository.GetByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetByIsbnAsync

    [Fact]
    public async Task GetByIsbnAsync_ExistingIsbn_ReturnsCorrectBookEdition()
    {
        // Act
        var result = await _repository.GetByIsbnAsync("978-0-14-023750-4"); 

        // Assert
        result.Should().NotBeNull();
        result!.ISBN.Should().Be("978-0-14-023750-4");
        result.Book!.OriginalTitle.Should().Be("The Catcher in the Rye");
        result.Publisher!.Name.Should().Be("Penguin Books");
        result.Language!.Name.Should().Be("English");
        result.Translators.Should().BeEmpty(); 
        result.Book.Authors.Should().ContainSingle(a => a.FullName == "J.D. Salinger");
        result.Book.Genres.Should().ContainSingle(g => g.Name == "Fiction");
    }

    [Fact]
    public async Task GetByIsbnAsync_NonExistingIsbn_ReturnsNull()
    {
        // Act
        var result = await _repository.GetByIsbnAsync("978-0-000000000-0");

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetPageAsync

    [Fact]
    public async Task GetPageAsync_ValidPage_ReturnsCorrectBookEditions()
    {
        // Act
        var result = await _repository.GetPageAsync(1, 2);

        // Assert
        result.Should().NotBeNull();
        result.Count.Should().Be(2); 

        var edition1 = result[0];
        edition1.Title.Should().Be("Faust");
        edition1.ISBN.Should().Be("978-0-521-31009-0");
        edition1.Book!.OriginalTitle.Should().Be("Faust");
        edition1.Publisher!.Name.Should().Be("HarperCollins");
        edition1.Translators.Should().ContainSingle(t => t.FullName == "Philip Wayne");
        edition1.Book.Authors.Should().ContainSingle(a => a.FullName == "Johann Wolfgang von Goethe");
        edition1.Book.Genres.Should().ContainSingle(g => g.Name == "Philosophy");

        var edition2 = result[1];
        edition2.ISBN.Should().Be("978-0-521-31009-2");
        edition2.Title.Should().Be("Faust");
        edition2.Book!.OriginalTitle.Should().Be("Faust");
        edition2.Publisher!.Name.Should().Be("HarperCollins");
        edition2.Translators.Should().BeEmpty();
        edition2.Book.Authors.Should().ContainSingle(a => a.FullName == "Johann Wolfgang von Goethe");
        edition2.Book.Genres.Should().ContainSingle(g => g.Name == "Philosophy");
    }

    [Fact]
    public async Task GetPageAsync_EmptyPage_ReturnsEmptyList()
    {
        // Act
        var result = await _repository.GetPageAsync(10, 2);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    #endregion

    #region GetPageByGenreAsync

    [Fact]
    public async Task GetPageByGenreAsync_ValidGenre_ReturnsCorrectBookEditions()
    {
        // Act
        var result = await _repository.GetPageByGenreAsync(1); 

        // Assert
        result.Should().NotBeNull();
        result.Count.Should().Be(1);

        var edition = result[0];
        edition.BookEditionID.Should().Be(1);
        edition.Book!.OriginalTitle.Should().Be("The Catcher in the Rye");
        edition.Book.Authors.Should().ContainSingle(a => a.FullName == "J.D. Salinger");
        result[0].Book!.Genres.Should().Contain(g => g.GenreID == 1 && g.Name == "Fiction");
    }

    [Fact]
    public async Task GetPageByGenreAsync_InvalidGenre_ReturnsEmptyList()
    {
        // Act
        var result = await _repository.GetPageByGenreAsync(999); 

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    #endregion

    #region GetPageByLanguageAsync

    [Fact]
    public async Task GetPageByLanguageAsync_ValidLanguageCode_ReturnsCorrectBookEditions()
    {
        // Act
        var result = await _repository.GetPageByLanguageAsync("EN");

        // Assert
        result.Should().NotBeNull();
        result.Count.Should().Be(1);

        var edition = result[0];
        edition.BookEditionID.Should().Be(1);
        edition.Book!.OriginalTitle.Should().Be("The Catcher in the Rye");
        edition.Book.Authors.Should().ContainSingle(a => a.FullName == "J.D. Salinger");
        result[0].LanguageCode.Should().Be("EN");
    }

    [Fact]
    public async Task GetPageByLanguageAsync_InvalidLanguageCode_ReturnsEmptyList()
    {
        // Act
        var result = await _repository.GetPageByLanguageAsync("ZZ");

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    #endregion

    #region GetPageByTitleAsync

    [Fact]
    public async Task GetPageByTitleAsync_ValidTitle_ReturnsCorrectBookEditions()
    {
        // Act
        var result = await _repository.GetPageByTitleAsync("Faust");

        // Assert
        result.Count.Should().Be(2);
        result.TrueForAll(e => e.Title == "Faust");
    }

    [Fact]
    public async Task GetPageByTitleAsync_NonExistingTitle_ReturnsEmptyList()
    {
        // Act
        var result = await _repository.GetPageByTitleAsync("NonExistingTitle");

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    #endregion

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            DbSeedHelper.DropTables(_connectionString);
        }

        _disposed = true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
