using ForeignLiteratureLibrary.BLL.Constants;
using ForeignLiteratureLibrary.BLL.Dtos;
using ForeignLiteratureLibrary.BLL.Interfaces;
using ForeignLiteratureLibrary.DAL.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace ForeignLiteratureLibrary.Web.Controllers;

public class GenresController : Controller
{
    private readonly IGenreService _genreService;

    public GenresController(IGenreService genreService)
    {
        _genreService = genreService;
    }

    public async Task<IActionResult> Index(int page = 1, int pageSize = PaginationConstants.DefaultPageSize)
    {
        var genres = await _genreService.GetGenresPageAsync(page, pageSize);

        return View(genres);
    }

    [HttpGet]
    public IActionResult Add()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Add(GenreDto genreDto)
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
            await _genreService.AddGenreAsync(genreDto);
        }
        catch (UniqueConstraintViolationException)
        {
            ViewBag.Errors = new List<string>() { "Цей жанр вже існує." };
            return View();
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var genre = await _genreService.GetGenreByIdAsync(id);

        if (genre == null)
        {
            return RedirectToAction(nameof(Index));
        }

        return View(genre);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(GenreDto genreDto)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Errors = ModelState.Values
                                .SelectMany(v => v.Errors)
                                .Select(e => e.ErrorMessage)
                                .ToList();
            return View(genreDto);
        }

        try
        {
            await _genreService.UpdateGenreAsync(genreDto);
        }
        catch (UniqueConstraintViolationException)
        {
            ViewBag.Errors = new List<string>() { "Цей жанр вже існує." };
            return View(genreDto);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var genre = await _genreService.GetGenreByIdAsync(id);

        if (genre == null)
        {
            return RedirectToAction(nameof(Index));
        }

        return View(genre);
    }

    [HttpPost]
    public async Task<IActionResult> Delete(GenreDto genreDto)
    {
        await _genreService.DeleteGenreAsync(genreDto.GenreID);

        return RedirectToAction(nameof(Index));
    }
}
