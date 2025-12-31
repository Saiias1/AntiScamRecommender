using AntiScamAPI.Services;
using AntiScamAPI.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Configure Kestrel to use Railway's PORT or default to 8080
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(int.Parse(port));
});

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Add PostgreSQL DbContext
var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL");
if (string.IsNullOrEmpty(connectionString))
{
    // Fallback for local development
    connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
}

// Railway PostgreSQL URLs need to be converted from postgres:// to the EF Core format
if (!string.IsNullOrEmpty(connectionString) && connectionString.StartsWith("postgres://"))
{
    connectionString = connectionString.Replace("postgres://", "");
    var parts = connectionString.Split('@');
    var userInfo = parts[0].Split(':');
    var hostInfo = parts[1].Split('/');
    var hostPort = hostInfo[0].Split(':');

    connectionString = $"Host={hostPort[0]};Port={hostPort[1]};Database={hostInfo[1]};Username={userInfo[0]};Password={userInfo[1]};SSL Mode=Require;Trust Server Certificate=true";
}

builder.Services.AddDbContext<AntiScamDbContext>(options =>
    options.UseNpgsql(connectionString));

// Add DatabaseService as Scoped
builder.Services.AddScoped<DatabaseService>();

// Register HybridRecommendationService as Singleton
builder.Services.AddSingleton<HybridRecommendationService>(sp =>
{
    var modelPath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "model.zip");
    var dataPath = Path.Combine(Directory.GetCurrentDirectory(), "Data");

    // Verify files exist
    if (!File.Exists(modelPath))
    {
        throw new FileNotFoundException($"Model file not found at {modelPath}");
    }

    var requiredFiles = new[] { "modules.csv", "users.csv", "ratings.csv" };
    foreach (var file in requiredFiles)
    {
        var filePath = Path.Combine(dataPath, file);
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Data file not found at {filePath}");
        }
    }

    var logger = sp.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("Loading hybrid recommendation service...");
    logger.LogInformation("  Model path: {ModelPath}", modelPath);
    logger.LogInformation("  Data path: {DataPath}", dataPath);

    var service = new HybridRecommendationService(modelPath, dataPath);

    logger.LogInformation("Hybrid recommendation service loaded successfully!");
    logger.LogInformation("  Configuration: 70% Collaborative + 30% Content-Based");

    return service;
});

var app = builder.Build();

// Run database migrations and seeding
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var dbLogger = services.GetRequiredService<ILogger<Program>>();

    try
    {
        var context = services.GetRequiredService<AntiScamDbContext>();

        dbLogger.LogInformation("Ensuring database is created...");
        await context.Database.EnsureCreatedAsync();
        dbLogger.LogInformation("Database ready");

        // Seed initial data from CSV files
        var dataPath = Path.Combine(Directory.GetCurrentDirectory(), "Data");
        var seeder = new DatabaseSeeder(context, services.GetRequiredService<ILogger<DatabaseSeeder>>(), dataPath);
        await seeder.SeedAsync();
    }
    catch (Exception ex)
    {
        dbLogger.LogError(ex, "An error occurred while migrating or seeding the database");
        throw;
    }
}

// Configure the HTTP request pipeline

app.UseCors("AllowAll");

// Only use HTTPS redirection in development (Railway handles SSL)
if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthorization();

app.MapControllers();

// Log startup info
var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("================================================================================");
logger.LogInformation("            ANTI-SCAM RECOMMENDER API - HYBRID ML.NET SYSTEM");
logger.LogInformation("================================================================================");
logger.LogInformation("API is running with HYBRID recommendation system:");
logger.LogInformation("  • 70% Collaborative Filtering (Matrix Factorization)");
logger.LogInformation("  • 30% Content-Based Filtering (Feature Matching)");
logger.LogInformation("");
logger.LogInformation("Available endpoints:");
logger.LogInformation("  • POST /api/recommendations - Get hybrid recommendations");
logger.LogInformation("  • POST /api/users/register - Register new user");
logger.LogInformation("  • GET  /api/users/{{userId}} - Get user profile");
logger.LogInformation("  • GET  /api/modules/{{moduleId}} - Get module details");
logger.LogInformation("  • GET  /api/modules - Get all modules");
logger.LogInformation("  • POST /api/ratings - Submit rating");
logger.LogInformation("  • GET  /api/health - Health check");
logger.LogInformation("================================================================================");

app.Run();
