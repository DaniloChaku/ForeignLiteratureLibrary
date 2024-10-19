using ForeignLiteratureLibrary.BLL.Constants;
using ForeignLiteratureLibrary.BLL.Dtos;
using ForeignLiteratureLibrary.BLL.Interfaces;
using ForeignLiteratureLibrary.BLL.Services;
using ForeignLiteratureLibrary.DAL.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace ForeignLiteratureLibrary.Web.Controllers;
public class LanguagesController : Controller
{
    private readonly ILanguageService _languageService;

    public LanguagesController(ILanguageService languageService)
    {
        _languageService = languageService;
    }

    public async Task<IActionResult> Index(int page = 1, int pageSize = PaginationConstants.DefaultPageSize)
    {
        var languages = await _languageService.GetLanguagesPageAsync(page, pageSize);

        return View(languages);
    }

    [HttpGet]
    public IActionResult Add()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Add(LanguageDto languageDto)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Errors = ModelState.Values
                                .SelectMany(v => v.Errors)
                                .Select(e => e.ErrorMessage)
                                .ToList();
            return View();
        }

        try
        {
            await _languageService.AddLanguageAsync(languageDto);
        }
        catch (UniqueConstraintViolationException)
        {
            ViewBag.Errors = new List<string>() { "Вже інує мова з такою назвою." };
            return View();
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var language = await _languageService.GetLanguageByIdAsync(id);

        if (language == null)
        {
            return RedirectToAction(nameof(Index));
        }

        return View(language);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(LanguageDto languageDto)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Errors = ModelState.Values
                                .SelectMany(v => v.Errors)
                                .Select(e => e.ErrorMessage)
                                .ToList();
            return View(languageDto);
        }

        try
        {
            await _languageService.UpdateLanguageAsync(languageDto);
        }
        catch (UniqueConstraintViolationException)
        {
            ViewBag.Errors = new List<string>() { "Вже інує мова з такою назвою." };
            return View(languageDto);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var language = await _languageService.GetLanguageByIdAsync(id);

        if (language == null)
        {
            return RedirectToAction(nameof(Index));
        }

        return View(language);
    }

    [HttpPost]
    public async Task<IActionResult> Delete(LanguageDto languageDto)
    {
        try
        {
            await _languageService.DeleteLanguageAsync(languageDto.LanguageID);
        }
        catch (ForeignKeyViolationException)
        {
            ViewBag.Errors = new List<string>() { "Видалення неможливо. Є книги, що написані цією мовою." };
            return View(languageDto);
        }

        return RedirectToAction(nameof(Index));
    }
}
