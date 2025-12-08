using Study.API.Data.Repositories;
using Study.API.Data.Repositories.Contracts;
using Microsoft.EntityFrameworkCore;
using study_app.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.Configure<RouteOptions>(options =>
{
    options.LowercaseUrls = true;
    options.LowercaseQueryStrings = true;
});

builder.Services.AddScoped<IStudyGroupRepository, StudyGroupRepository>();

builder.Services.AddDbContext<StudyGroupDbContext>(options =>
    options.UseInMemoryDatabase("StudyGroupDb"));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();


// Seed the database
app.SeedDatabase();

app.Run();

public partial class Program { }
