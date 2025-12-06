using Study.API.Data.Models;

namespace study_app.Extensions;

public static class DatabaseExtensions
{
    public static IApplicationBuilder SeedDatabase(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<StudyGroupDbContext>();
            
        context.Database.EnsureCreated();

        if (!context.Users.Any())
        {
            context.Users.AddRange(
                new User { UserId = 1, Name = "John Doe", Email = "john@example.com" },
                new User { UserId = 2, Name = "Jane Smith", Email = "jane@example.com" },
                new User { UserId = 3, Name = "Bob Wilson", Email = "bob@example.com" }
            );
            context.SaveChanges();
        }

        return app;
    }
}