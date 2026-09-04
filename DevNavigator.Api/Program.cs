using DevNavigator.Api.Data;
using DevNavigator.Api.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddPolicy("DevNavigatorWeb", policy =>
    {
        policy
            .WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(
        builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IIndexService, IndexService>();
builder.Services.AddScoped<ISearchService, SearchService>();
builder.Services.AddScoped<CodeSymbolExtractor>();
builder.Services.AddScoped<CodeSymbolRelationshipBuilder>();
builder.Services.AddScoped<ImportResolver>();
builder.Services.AddScoped<ServiceNavigationService>();

// Built-in OpenAPI
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Comment this for now
// app.UseHttpsRedirection();

app.UseCors("DevNavigatorWeb");

app.UseAuthorization();

app.MapControllers();

app.MapGet("/", () => new
{
    application = "DevNavigator AI",
    status = "Running",
    message = "Developer navigation API is running."
});

app.Run();