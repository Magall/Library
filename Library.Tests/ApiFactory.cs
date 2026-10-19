using Library.Database;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Time.Testing;

namespace Library.Tests;

public class ApiFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");
    public FakeTimeProvider Time { get; } =
        new(new DateTimeOffset(2026, 10, 12, 9, 0, 0, TimeSpan.Zero));

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {

        _connection.Open();

        builder.ConfigureServices(services =>
        {
            // 1. Find the existing DbContextOptions<LibraryDbContext> descriptor
            //    in `services` and remove it. 

            services.RemoveAll<DbContextOptions<LibraryDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<LibraryDbContext>>();
            services.RemoveAll<TimeProvider>();
            services.AddSingleton<TimeProvider>(Time);


            // 2. Register LibraryDbContext again, using SQLite on _connection.   
            services.AddDbContext<LibraryDbContext>(options =>
               options.UseSqlite(_connection));
            // 3. Build a service provider, create a scope, resolve LibraryDbContext,
            using var provider = services.BuildServiceProvider();
            using var scope = provider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
            //    and call EnsureCreated().   
            db.Database.EnsureCreated();
        });
        builder.ConfigureAppConfiguration((_, config) =>
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Lends:MaxSimultaneousLends"] = "3",
                ["Lends:LendingDurationInWorkingDays"] = "5",
            }
        ));

    }

    protected override void Dispose(bool disposing)
    {
        // 4. Close/dispose _connection.   
        if (disposing)
        {

            _connection.Dispose();
        }

        base.Dispose(disposing);

    }
}
