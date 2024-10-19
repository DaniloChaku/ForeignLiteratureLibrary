using ForeignLiteratureLibrary.BLL.Constants;
using ForeignLiteratureLibrary.BLL.Dtos;
using ForeignLiteratureLibrary.BLL.Interfaces;
using ForeignLiteratureLibrary.DAL.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace ForeignLiteratureLibrary.Web.Controllers;
public class CountriesController : Controller
{
    private readonly ICountryService _countryService;

    public CountriesController(ICountryService countryService)
    {
        _countryService = countryService;
    }

    public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = PaginationConstants.DefaultPageSize)
    {
        var countries = await _countryService.GetCountriesPageAsync(pageNumber, pageSize);

        return View(countries);
    }

    [HttpGet]
    public IActionResult Add()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Add(CountryDto countryDto)
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
            await _countryService.AddCountryAsync(countryDto);
        }
        catch (UniqueConstraintViolationException)
        {
            ViewBag.Errors = new List<string>() { "Вже інує країна з такою назвою." };
            return View();
        }
        
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var country = await _countryService.GetCountryByIdAsync(id);

        if (country == null)
        {
            return RedirectToAction(nameof(Index));
        }

        return View(country);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(CountryDto countryDto)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Errors = ModelState.Values
                                .SelectMany(v => v.Errors)
                                .Select(e => e.ErrorMessage)
                                .ToList();
            return View(countryDto);
        }

        try
        {
            await _countryService.UpdateCountryAsync(countryDto);
        }
        catch (UniqueConstraintViolationException)
        {
            ViewBag.Errors = new List<string>() { "Вже інує країна з такою назвою." };
            return View(countryDto);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var country = await _countryService.GetCountryByIdAsync(id);

        if (country == null)
        {
            return RedirectToAction(nameof(Index));
        }

        return View(country);
    }

    [HttpPost]
    public async Task<IActionResult> Delete(CountryDto countryDto)
    {
        try
        {
            await _countryService.DeleteCountryAsync(countryDto.CountryID);
        }
        catch (ForeignKeyViolationException)
        {
            ViewBag.Errors = new List<string>() { "Видалення неможливо. Є автори, перекладачі або видавництва, пов'язані з цією країною." };
            return View(countryDto);
        }
        
        return RedirectToAction(nameof(Index));
    }
}
