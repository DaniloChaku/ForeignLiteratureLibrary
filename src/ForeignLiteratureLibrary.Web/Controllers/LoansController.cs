using ForeignLiteratureLibrary.BLL.Constants;
using ForeignLiteratureLibrary.BLL.Dtos;
using ForeignLiteratureLibrary.BLL.Interfaces;
using ForeignLiteratureLibrary.DAL.Exceptions;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc;

namespace ForeignLiteratureLibrary.Web.Controllers;

public class LoansController : Controller
{
    private readonly ILoanService _bookEditionLoanService;
    private readonly IBookEditionService _bookEditionService;
    private readonly IReaderService _readerService;

    public LoansController(
        ILoanService bookEditionLoanService,
        IBookEditionService bookEditionService,
        IReaderService readerService)
    {
        _bookEditionLoanService = bookEditionLoanService;
        _bookEditionService = bookEditionService;
        _readerService = readerService;
    }

    public async Task<IActionResult> Index(int page = 1, int pageSize = PaginationConstants.DefaultPageSize, bool showOverdueOnly = false)
    {
        PaginatedResult<LoanDto> loans;

        if (showOverdueOnly)
        {
            loans = await _bookEditionLoanService.GetOverdueLoansPageAsync(page, pageSize);
        }
        else
        {
            loans = await _bookEditionLoanService.GetLoansPageAsync(page, pageSize);
        }

        ViewBag.ShowOverdueOnly = showOverdueOnly;
        return View(loans);
    }

    [HttpGet]
    public async Task<IActionResult> Add()
    {
        await PopulateViewBagAsync();

        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Add(LoanDto loan)
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

        try
        {
            await _bookEditionLoanService.AddLoanAsync(loan);
        }
        catch (BookEditionUnavailableException)
        {
            await PopulateViewBagAsync();
            ViewBag.Errors = new List<string>() { "Немає доступних примірників." };
            return View(loan);
        }
        catch (CheckConstraintViolationException ex)
        {
            await PopulateViewBagAsync();
            ViewBag.Errors = new List<string>() { ex.Message };
            return View(loan);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var loan = await _bookEditionLoanService.GetLoanByIdAsync(id);

        if (loan == null)
        {
            return RedirectToAction(nameof(Index));
        }


        await PopulateViewBagAsync();

        return View(loan);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(LoanDto loan)
    {
        if (!ModelState.IsValid)
        {
            await PopulateViewBagAsync();
            ViewBag.Errors = ModelState.Values
                                .SelectMany(v => v.Errors)
                                .Select(e => e.ErrorMessage)
                                .ToList();
            return View(loan);
        }


        try
        {
            await _bookEditionLoanService.UpdateLoanAsync(loan);
        }
        catch (BookEditionUnavailableException)
        {
            await PopulateViewBagAsync();
            ViewBag.Errors = new List<string>() { "Немає доступних примірників." };
            return View(loan);
        }
        catch (CheckConstraintViolationException ex)
        {
            await PopulateViewBagAsync();
            ViewBag.Errors = new List<string>() { ex.Message };
            return View(loan);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var loan = await _bookEditionLoanService.GetLoanByIdAsync(id);

        if (loan == null)
        {
            return RedirectToAction(nameof(Index));
        }

        await PopulateViewBagAsync();

        return View(loan);
    }

    [HttpPost]
    public async Task<IActionResult> Delete(LoanDto loan)
    {
        await _bookEditionLoanService.DeleteLoanAsync(loan.LoanID);


        return RedirectToAction(nameof(Index));
    }

    private async Task<List<SelectListItem>> GetBookEditionListItems()
    {
        var bookEditions = await _bookEditionService.GetAllBookEditionsAsync();
        return bookEditions.Select(be => new SelectListItem
        {
            Value = be.BookEditionID.ToString(),
            Text = $"{be.ISBN}: {be.Title}, {be.Language?.Name}"
        }).ToList();
    }

    private async Task<List<SelectListItem>> GetReaderListItems()
    {
        var readers = await _readerService.GetAllReadersAsync();
        return readers.Select(r => new SelectListItem
        {
            Value = r.ReaderID.ToString(),
            Text = $"{r.LibraryCardNumber}: {r.FullName}"
        }).ToList();
    }

    private async Task PopulateViewBagAsync()
    {
        ViewBag.BookEditions = await GetBookEditionListItems();
        ViewBag.Readers = await GetReaderListItems();
    }
}
