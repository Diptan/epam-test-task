using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Study.API.IntegrationTests.Tests.Infrastructure
{
    // Requires: public partial class Program in API project
    public class TestStudyGroupFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<StudyGroupDbContext>));

                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }
                services.AddDbContext<StudyGroupDbContext>(options =>
                {
                    options.UseInMemoryDatabase($"StudyGroupDb_Test_{Guid.NewGuid()}");
                });
            });
        }
    }
}