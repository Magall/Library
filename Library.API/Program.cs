using Library.Database;
using Library.Repositories;
using Library.Services;
using Library.Settings;
using Mapster;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddMapster();
builder.Services.AddOpenApi();

builder.Services.AddScoped<IBookService, BookService>();
builder.Services.AddScoped<IBookRepository, BookRepository>();
builder.Services.AddScoped<ILendsService, LendsService>();
builder.Services.AddScoped<ILendsRepository,LendsRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.Configure<LendsOptions>(builder.Configuration.GetSection(LendsOptions.SectionName));
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddDbContext<LibraryDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
);
builder.Services.Configure<LendsOptions>(
    builder.Configuration.GetSection(LendsOptions.SectionName));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "Library API v1");
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
