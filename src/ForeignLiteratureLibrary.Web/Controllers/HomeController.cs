using ForeignLiteratureLibrary.BLL.Interfaces;
using ForeignLiteratureLibrary.Web.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace ForeignLiteratureLibrary.Web.Controllers;
public class HomeController : Controller
{
    private readonly IAuthorService _authorService;
    private readonly IBookService _bookService;

    public HomeController(IAuthorService authorService, IBookService bookService)
    {
        this._authorService = authorService;
        this._bookService = bookService;
    }

    public async Task<IActionResult> Index()
    {
        var startOfCurrentMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var startOfPreviousMonth = startOfCurrentMonth.AddMonths(-1);

        var authors = await _authorService.GetTop10AuthorsAsync(startOfPreviousMonth, startOfCurrentMonth);
        var books = await _bookService.GetTop10BooksAsync(startOfPreviousMonth, startOfCurrentMonth);

        var viewModel = new TopBooksAndAuthorsViewModel()
        {
            TopAuthors = authors,
            TopBooks = books
        };

        return View(viewModel);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
