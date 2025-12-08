using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Study.Integration.Tests.Infrastructure;

public class StudyGroupApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // 1) Remove existing DbContext registration from Program.cs
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<StudyGroupDbContext>));

            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // 2) Register a test-specific in-memory DB with a fixed name
            //    so all contexts in this host share the same data
            services.AddDbContext<StudyGroupDbContext>(options =>
            {
                options.UseInMemoryDatabase("StudyGroupDb_Test");
            });

            // 3) Build a temporary provider to ensure DB is clean for each host
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<StudyGroupDbContext>();

            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();
        });
    }
}