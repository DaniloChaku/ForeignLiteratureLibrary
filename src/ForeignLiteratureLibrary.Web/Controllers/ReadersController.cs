using ForeignLiteratureLibrary.BLL.Constants;
using ForeignLiteratureLibrary.BLL.Dtos;
using ForeignLiteratureLibrary.BLL.Interfaces;
using ForeignLiteratureLibrary.DAL.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace ForeignLiteratureLibrary.Web.Controllers;

public class ReadersController : Controller
{
    private readonly IReaderService _readerService;

    public ReadersController(IReaderService readerService)
    {
        _readerService = readerService;
    }

    public async Task<IActionResult> Index(int page = 1, int pageSize = PaginationConstants.DefaultPageSize)
    {
        var readers = await _readerService.GetReadersPageAsync(page, pageSize);

        return View(readers);
    }

    [HttpGet]
    public IActionResult Add()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Add(ReaderDto readerDto)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Errors = ModelState.Values
                                .SelectMany(v => v.Errors)
                                .Select(e => e.ErrorMessage)
                                .ToList();
            return View();
        }

        await _readerService.AddReaderAsync(readerDto);

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(string id)
    {
        var reader = await _readerService.GetReaderByLibraryCardNumberAsync(id);

        if (reader == null)
        {
            return RedirectToAction(nameof(Index));
        }

        return View(reader);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(ReaderDto readerDto)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Errors = ModelState.Values
                                .SelectMany(v => v.Errors)
                                .Select(e => e.ErrorMessage)
                                .ToList();
            return View(readerDto);
        }

        try
        {
            await _readerService.UpdateReaderAsync(readerDto);
        }
        catch (UniqueConstraintViolationException)
        {
            ViewBag.Errors = new List<string>() { "Вже інує країна з такою назвою або кодом." };
            return View(readerDto);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Delete(string id)
    {
        var reader = await _readerService.GetReaderByLibraryCardNumberAsync(id);

        if (reader == null)
        {
            return RedirectToAction(nameof(Index));
        }

        return View(reader);
    }

    [HttpPost]
    public async Task<IActionResult> Delete(ReaderDto readerDto)
    {
        try
        {
            await _readerService.DeleteReaderAsync(readerDto.LibraryCardNumber);
        }
        catch (ForeignKeyViolationException)
        {
            ViewBag.Errors = new List<string>() { "Видалення неможливо, бо є записи, як цей читач брав книги." };
            return View(readerDto);
        }

        return RedirectToAction(nameof(Index));
    }
}
