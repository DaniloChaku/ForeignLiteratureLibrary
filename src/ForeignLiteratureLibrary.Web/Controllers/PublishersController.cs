using ForeignLiteratureLibrary.BLL.Constants;
using ForeignLiteratureLibrary.BLL.Dtos;
using ForeignLiteratureLibrary.BLL.Interfaces;
using ForeignLiteratureLibrary.DAL.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ForeignLiteratureLibrary.Web.Controllers;
public class PublishersController : Controller
{
    private readonly IPublisherService _publisherService;
    private readonly ICountryService _countryService;

    public PublishersController(IPublisherService publisherService, ICountryService countryService)
    {
        _publisherService = publisherService;
        _countryService = countryService;
    }

    public async Task<IActionResult> Index(int page = 1, int pageSize = PaginationConstants.DefaultPageSize)
    {
        var publishers = await _publisherService.GetPublishersPageAsync(page, pageSize);

        return View(publishers);
    }

    [HttpGet]
    public async Task<IActionResult> Add()
    {
        await PopulateViewBagAsync();

        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Add(PublisherDto publisherDto)
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

        await _publisherService.AddPublisherAsync(publisherDto);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var publisher = await _publisherService.GetPublisherByIdAsync(id);

        if (publisher == null)
        {
            return RedirectToAction(nameof(Index));
        }

        await PopulateViewBagAsync();

        return View(publisher);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(PublisherDto publisherDto)
    {
        if (!ModelState.IsValid)
        {
            await PopulateViewBagAsync();
            ViewBag.Errors = ModelState.Values
                                .SelectMany(v => v.Errors)
                                .Select(e => e.ErrorMessage)
                                .ToList();
            return View(publisherDto);
        }

        await _publisherService.UpdatePublisherAsync(publisherDto);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var publisher = await _publisherService.GetPublisherByIdAsync(id);

        if (publisher == null)
        {
            return RedirectToAction(nameof(Index));
        }

        return View(publisher);
    }

    [HttpPost]
    public async Task<IActionResult> Delete(PublisherDto publisherDto)
    {
        try
        {
            await _publisherService.DeletePublisherAsync(publisherDto.PublisherID);
        }
        catch (ForeignKeyViolationException)
        {
            return View(publisherDto);
        }
        
        return RedirectToAction(nameof(Index));
    }

    private async Task<List<SelectListItem>> GetCountryListItems()
    {
        var countries = await _countryService.GetAllCountriesAsync();
        return countries.Select(c => new SelectListItem
        {
            Value = c.CountryID.ToString(),
            Text = c.Name
        }).ToList();
    }

    private async Task PopulateViewBagAsync()
    {
        ViewBag.Countries = await GetCountryListItems();
    }
}
