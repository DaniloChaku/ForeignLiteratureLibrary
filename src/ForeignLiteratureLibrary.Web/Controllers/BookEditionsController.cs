using ForeignLiteratureLibrary.BLL.Constants;
using ForeignLiteratureLibrary.BLL.Dtos;
using ForeignLiteratureLibrary.BLL.Interfaces;
using ForeignLiteratureLibrary.DAL.Exceptions;
using ForeignLiteratureLibrary.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ForeignLiteratureLibrary.Web.Controllers;

public class BookEditionsController : Controller
{
    private readonly IBookEditionService _bookeditionService;
    private readonly ITranslatorService _translatorService;
    private readonly ILanguageService _languageService;
    private readonly IPublisherService _publisherService;
    private readonly IBookService _bookService;

    public BookEditionsController(
        IBookEditionService bookEditionService, 
        ITranslatorService translatorService,
        ILanguageService languageService,
        IPublisherService publisherService,
        IBookService bookService)
    {
        _bookeditionService = bookEditionService;
        _translatorService = translatorService;
        _languageService = languageService;
        _publisherService = publisherService;
        _bookService = bookService;
    }

    public async Task<IActionResult> Index(int page = 1, int pageSize = PaginationConstants.DefaultPageSize)
    {
        var bookeditions = await _bookeditionService.GetBookEditionsPageAsync(page, pageSize);
        return View(bookeditions);
    }

    [HttpGet]
    public async Task<IActionResult> Add()
    {
        await PopulateViewBagAsync();

        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Add(BookEditionWithTranslatorsViewModel bookEditionModel)
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

        bookEditionModel.PopulateBookEditionTranslators();

        await _bookeditionService.AddBookEditionAsync(bookEditionModel.BookEdition);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var bookEdition = await _bookeditionService.GetBookEditionByIdAsync(id);

        if (bookEdition == null)
        {
            return RedirectToAction(nameof(Index));
        }

        var bookeditionModel = new BookEditionWithTranslatorsViewModel()
        {
            BookEdition = bookEdition,
            Translators = bookEdition.Translators.Select(t => t.TranslatorID).ToList(),
        };

        await PopulateViewBagAsync();

        return View(bookeditionModel);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(BookEditionWithTranslatorsViewModel bookEditionModel)
    {
        if (!ModelState.IsValid)
        {
            await PopulateViewBagAsync();
            ViewBag.Errors = ModelState.Values
                                .SelectMany(v => v.Errors)
                                .Select(e => e.ErrorMessage)
                                .ToList();
            return View(bookEditionModel);
        }

        bookEditionModel.PopulateBookEditionTranslators();

        try
        {
            await _bookeditionService.UpdateBookEditionAsync(bookEditionModel.BookEdition);
        }
        catch (BookEditionUnavailableException)
        {
            await PopulateViewBagAsync();
            ViewBag.Errors = new List<string>() { "Не можна встановити кількість примірників меншу за кількість виданих примірників." };
            return View(bookEditionModel);
        }
        
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var bookEdition = await _bookeditionService.GetBookEditionByIdAsync(id);

        if (bookEdition == null)
        {
            return RedirectToAction(nameof(Index));
        }

        return View(bookEdition);
    }

    [HttpPost]
    public async Task<IActionResult> Delete(BookEditionDto bookEdition)
    {
        try
        {
            await _bookeditionService.DeleteBookEditionAsync(bookEdition.BookEditionID);
        }
        catch (ForeignKeyViolationException)
        {
            ViewBag.Errors = new List<string>() { "Видалення неможливо. Є дані про видачу цієї книги." };
            return View(bookEdition);
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task<List<SelectListItem>> GetTranslatorListItems()
    {
        var translators = await _translatorService.GetAllTranslatorsAsync();
        return translators.Select(t => new SelectListItem
        {
            Value = t.TranslatorID.ToString(),
            Text = t.FullName
        }).ToList();
    }

    private async Task<List<SelectListItem>> GetPublisherListItems()
    {
        var publishers = await _publisherService.GetAllPublishersAsync();
        return publishers.Select(p => new SelectListItem
        {
            Value = p.PublisherID.ToString(),
            Text = p.Name
        }).ToList();
    }

    private async Task<List<SelectListItem>> GetLanguageListItems()
    {
        var languages = await _languageService.GetAllLanguagesAsync();
        return languages.Select(l => new SelectListItem
        {
            Value = l.LanguageID.ToString(),
            Text = l.Name
        }).ToList();
    }

    private async Task<List<SelectListItem>> GetBookListItems()
    {
        var books = await _bookService.GetAllBooksAsync();
        return books.Select(b => new SelectListItem
        {
            Value = b.BookID.ToString(),
            Text = b.OriginalTitle
        }).ToList();
    }

    private async Task PopulateViewBagAsync()
    {
        ViewBag.Languages = await GetLanguageListItems();
        ViewBag.Translators = await GetTranslatorListItems();
        ViewBag.Publishers = await GetPublisherListItems();
        ViewBag.Books = await GetBookListItems();
    }
}
