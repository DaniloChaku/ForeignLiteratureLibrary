using ForeignLiteratureLibrary.BLL.Constants;
using ForeignLiteratureLibrary.BLL.Dtos;
using ForeignLiteratureLibrary.BLL.Interfaces;
using ForeignLiteratureLibrary.DAL.Exceptions;
using ForeignLiteratureLibrary.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ForeignLiteratureLibrary.Web.Controllers;

public class BooksController : Controller
{
    private readonly IBookService _bookService;
    private readonly IAuthorService _authorService;
    private readonly IGenreService _genreService;
    private readonly ILanguageService _languageService;

    public BooksController(IBookService bookService, IAuthorService authorService, IGenreService genreService, ILanguageService languageService)
    {
        _bookService = bookService;
        _authorService = authorService;
        _genreService = genreService;
        _languageService = languageService;
    }

    public async Task<IActionResult> Index(int page = 1, int pageSize = PaginationConstants.DefaultPageSize)
    {
        var books = await _bookService.GetBooksPageAsync(page, pageSize);
        return View(books);
    }

    [HttpGet]
    public async Task<IActionResult> Add()
    {
        await PopulateViewBagAsync();

        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Add(BookWithGenresAndAuthorsViewModel bookModel)
    {
        if (!ModelState.IsValid)
        {
            await PopulateViewBagAsync();
            ViewBag.Errors = ModelState.Values
                                .SelectMany(v => v.Errors)
                                .Select(e => e.ErrorMessage)
                                .ToList();
            return View();
        }

        bookModel.PopulateBookCollections();

        await _bookService.AddBookAsync(bookModel.Book);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var book = await _bookService.GetBookByIdAsync(id);

        if (book == null)
        {
            return RedirectToAction(nameof(Index));
        }

        var bookModel = new BookWithGenresAndAuthorsViewModel()
        {
            Book = book,
            Authors = book.Authors.Select(a => a.AuthorID).ToList(),
            Genres = book.Genres.Select(g => g.GenreID).ToList()
        };

        await PopulateViewBagAsync();

        return View(bookModel);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(BookWithGenresAndAuthorsViewModel bookModel)
    {
        if (!ModelState.IsValid)
        {
            await PopulateViewBagAsync();
            ViewBag.Errors = ModelState.Values
                                .SelectMany(v => v.Errors)
                                .Select(e => e.ErrorMessage)
                                .ToList();
            return View(bookModel);
        }

        bookModel.PopulateBookCollections();

        await _bookService.UpdateBookAsync(bookModel.Book);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var book = await _bookService.GetBookByIdAsync(id);

        if (book == null)
        {
            return RedirectToAction(nameof(Index));
        }

        return View(book);
    }

    [HttpPost]
    public async Task<IActionResult> Delete(BookDto bookModel)
    {
        try
        {
            await _bookService.DeleteBookAsync(bookModel.BookID);
        }
        catch (ForeignKeyViolationException)
        {
            ViewBag.Errors = new List<string>() { "Видалення неможливо. Є дані про видання цієї книги." };
            return View(bookModel);
        }


        return RedirectToAction(nameof(Index));
    }

    private async Task<List<SelectListItem>> GetAuthorListItems()
    {
        var authors = await _authorService.GetAllAuthorsAsync();
        return authors.Select(a => new SelectListItem
        {
            Value = a.AuthorID.ToString(),
            Text = a.AuthorFullName
        }).ToList();
    }

    private async Task<List<SelectListItem>> GetGenreListItems()
    {
        var genres = await _genreService.GetAllGenresAsync();
        return genres.Select(g => new SelectListItem
        {
            Value = g.GenreID.ToString(),
            Text = g.Name
        }).ToList();
    }

    private async Task<List<SelectListItem>> GetLanguageListItems()
    {
        var genres = await _languageService.GetAllLanguagesAsync();
        return genres.Select(g => new SelectListItem
        {
            Value = g.LanguageID.ToString(),
            Text = g.Name
        }).ToList();
    }

    private async Task PopulateViewBagAsync()
    {
        ViewBag.Languages = await GetLanguageListItems();
        ViewBag.Genres = await GetGenreListItems();
        ViewBag.Authors = await GetAuthorListItems();
    }
}
