using AntiScamAPI.Services;

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
