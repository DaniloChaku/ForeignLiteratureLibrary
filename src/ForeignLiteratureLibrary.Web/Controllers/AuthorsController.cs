using ForeignLiteratureLibrary.BLL.Constants;
using ForeignLiteratureLibrary.BLL.Dtos;
using ForeignLiteratureLibrary.BLL.Interfaces;
using ForeignLiteratureLibrary.BLL.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ForeignLiteratureLibrary.Web.Controllers;

public class AuthorsController : Controller
{
    private readonly IAuthorService _authorService;
    private readonly ICountryService _countryService;

    public AuthorsController(IAuthorService authorService, ICountryService countryService)
    {
        _authorService = authorService;
        _countryService = countryService;
    }

    public async Task<IActionResult> Index(int page = 1, int pageSize = PaginationConstants.DefaultPageSize)
    {
        var authors = await _authorService.GetAuthorsPageAsync(page, pageSize);

        return View(authors);
    }

    [HttpGet]
    public async Task<IActionResult> Add()
    {
        ViewBag.Countries = await GetCountryListItems();

        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Add(AuthorDto authorDto)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Countries = await GetCountryListItems();
            ViewBag.Errors = ModelState.Values
                                .SelectMany(v => v.Errors)
                                .Select(e => e.ErrorMessage)
                                .ToList();
            return View();
        }

        await _authorService.AddAuthorAsync(authorDto);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var author = await _authorService.GetAuthorByIdAsync(id);

        if (author == null)
        {
            return RedirectToAction(nameof(Index));
        }

        ViewBag.Countries = await GetCountryListItems();

        return View(author);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(AuthorDto authorDto)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Countries = await GetCountryListItems();
            ViewBag.Errors = ModelState.Values
                                .SelectMany(v => v.Errors)
                                .Select(e => e.ErrorMessage)
                                .ToList();
            return View(authorDto);
        }

        await _authorService.UpdateAuthorAsync(authorDto);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var author = await _authorService.GetAuthorByIdAsync(id);

        if (author == null)
        {
            return RedirectToAction(nameof(Index));
        }

        return View(author);
    }

    [HttpPost]
    public async Task<IActionResult> Delete(AuthorDto authorDto)
    {
        await _authorService.DeleteAuthorAsync(authorDto.AuthorID);

        return RedirectToAction(nameof(Index));
    }

    private async Task<List<SelectListItem>> GetCountryListItems()
    {
        var countries = await _countryService.GetAllCountriesAsync();
        return countries.Select(
            c => new SelectListItem()
            {
                Value = c.CountryCode,
                Text = c.Name
            }).ToList();
    }
}
