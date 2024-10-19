using ForeignLiteratureLibrary.BLL.Constants;
using ForeignLiteratureLibrary.BLL.Dtos;
using ForeignLiteratureLibrary.BLL.Interfaces;
using ForeignLiteratureLibrary.DAL.Exceptions;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc;

namespace ForeignLiteratureLibrary.Web.Controllers;

public class LoansController : Controller
{
    private readonly IBookEditionLoanService _bookEditionLoanService;
    private readonly IBookEditionService _bookEditionService;
    private readonly IReaderService _readerService;

    public LoansController(
        IBookEditionLoanService bookEditionLoanService,
        IBookEditionService bookEditionService,
        IReaderService readerService)
    {
        _bookEditionLoanService = bookEditionLoanService;
        _bookEditionService = bookEditionService;
        _readerService = readerService;
    }

    public async Task<IActionResult> Index(int page = 1, int pageSize = PaginationConstants.DefaultPageSize)
    {
        var bookEditionLoans = await _bookEditionLoanService.GetLoansPageAsync(page, pageSize);
        return View(bookEditionLoans);
    }

    [HttpGet]
    public async Task<IActionResult> Add()
    {
        await PopulateViewBagAsync();

        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Add(LoanDto bookEditionLoan)
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
            await _bookEditionLoanService.AddLoanAsync(bookEditionLoan);
        }
        catch (BookEditionUnavailableException)
        {
            await PopulateViewBagAsync();
            ViewBag.Errors = new List<string>() { "Немає доступних примірників." };
            return View(bookEditionLoan);
        }
        catch (CheckConstraintViolationException ex)
        {
            await PopulateViewBagAsync();
            ViewBag.Errors = new List<string>() { ex.Message };
            return View(bookEditionLoan);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var bookEditionLoan = await _bookEditionLoanService.GetLoanByIdAsync(id);

        if (bookEditionLoan == null)
        {
            return RedirectToAction(nameof(Index));
        }


        await PopulateViewBagAsync();

        return View(bookEditionLoan);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(LoanDto bookEditionLoan)
    {
        if (!ModelState.IsValid)
        {
            await PopulateViewBagAsync();
            ViewBag.Errors = ModelState.Values
                                .SelectMany(v => v.Errors)
                                .Select(e => e.ErrorMessage)
                                .ToList();
            return View(bookEditionLoan);
        }


        try
        {
            await _bookEditionLoanService.UpdateLoanAsync(bookEditionLoan);
        }
        catch (BookEditionUnavailableException)
        {
            await PopulateViewBagAsync();
            ViewBag.Errors = new List<string>() { "Немає доступних примірників." };
            return View(bookEditionLoan);
        }
        catch (CheckConstraintViolationException ex)
        {
            await PopulateViewBagAsync();
            ViewBag.Errors = new List<string>() { ex.Message };
            return View(bookEditionLoan);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var bookeditionloan = await _bookEditionLoanService.GetLoanByIdAsync(id);

        if (bookeditionloan == null)
        {
            return RedirectToAction(nameof(Index));
        }

        await PopulateViewBagAsync();

        return View(bookeditionloan);
    }

    [HttpPost]
    public async Task<IActionResult> Delete(LoanDto bookEditionLoan)
    {
        await _bookEditionLoanService.DeleteLoanAsync(bookEditionLoan.LoanID);


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
