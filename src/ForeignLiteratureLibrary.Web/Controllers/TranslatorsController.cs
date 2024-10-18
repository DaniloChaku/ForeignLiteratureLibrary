using ForeignLiteratureLibrary.BLL.Constants;
using ForeignLiteratureLibrary.BLL.Dtos;
using ForeignLiteratureLibrary.BLL.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ForeignLiteratureLibrary.Web.Controllers;

public class TranslatorsController : Controller
{
    private readonly ITranslatorService _translatorService;

    public TranslatorsController(ITranslatorService translatorService)
    {
        _translatorService = translatorService;
    }

    public async Task<IActionResult> Index(int page = 1, int pageSize = PaginationConstants.DefaultPageSize)
    {
        var translators = await _translatorService.GetTranslatorsPageAsync(page, pageSize);

        return View(translators);
    }

    [HttpGet]
    public async Task<IActionResult> Add()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Add(TranslatorDto translatorDto)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Errors = ModelState.Values
                                .SelectMany(v => v.Errors)
                                .Select(e => e.ErrorMessage)
                                .ToList();
            return View();
        }

        await _translatorService.AddTranslatorAsync(translatorDto);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var translator = await _translatorService.GetTranslatorByIdAsync(id);

        if (translator == null)
        {
            return RedirectToAction(nameof(Index));
        }

        return View(translator);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(TranslatorDto translatorDto)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Errors = ModelState.Values
                                .SelectMany(v => v.Errors)
                                .Select(e => e.ErrorMessage)
                                .ToList();
            return View(translatorDto);
        }

        await _translatorService.UpdateTranslatorAsync(translatorDto);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var translator = await _translatorService.GetTranslatorByIdAsync(id);

        if (translator == null)
        {
            return RedirectToAction(nameof(Index));
        }

        return View(translator);
    }

    [HttpPost]
    public async Task<IActionResult> Delete(TranslatorDto translatorDto)
    {
        await _translatorService.DeleteTranslatorAsync(translatorDto.TranslatorID);

        return RedirectToAction(nameof(Index));
    }
}
