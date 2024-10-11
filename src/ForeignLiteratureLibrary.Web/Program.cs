using ForeignLiteratureLibrary.BLL.Interfaces;
using ForeignLiteratureLibrary.BLL.Services;
using ForeignLiteratureLibrary.DAL.Interfaces;
using ForeignLiteratureLibrary.DAL.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

var connectionString = builder.Configuration.GetConnectionString("Default")!;

builder.Services.AddScoped<IAuthorRepository>(provider => new AuthorRepository(connectionString))
    .AddScoped<IBookEditionLoanRepository>(provider => new BookEditionLoanRepository(connectionString))
    .AddScoped<IBookEditionRepository>(provider => new BookEditionRepository(connectionString))
    .AddScoped<IBookRepository>(provider => new BookRepository(connectionString))
    .AddScoped<ICountryRepository>(provider => new CountryRepository(connectionString))
    .AddScoped<IGenreRepository>(provider => new GenreRepository(connectionString))
    .AddScoped<ILanguageRepository>(provider => new LanguageRepository(connectionString))
    .AddScoped<IPublisherRepository>(provider => new PublisherRepository(connectionString))
    .AddScoped<IReaderRepository>(provider => new ReaderRepository(connectionString))
    .AddScoped<ITranslatorRepository>(provider => new TranslatorRepository(connectionString));

builder.Services.AddScoped<IAuthorService, AuthorService>()
    .AddScoped<IBookEditionLoanService, BookEditionLoanService>()
    .AddScoped<IBookEditionService, BookEditionService>()
    .AddScoped<IBookService, BookService>()
    .AddScoped<ICountryService, CountryService>()
    .AddScoped<IGenreService, GenreService>()
    .AddScoped<ILanguageService, LanguageService>()
    .AddScoped<IPublisherService, PublisherService>()
    .AddScoped<IReaderService, ReaderService>()
    .AddScoped<ITranslatorService, TranslatorService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
