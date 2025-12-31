using Microsoft.EntityFrameworkCore;
using Microsoft.ML;
using AntiScamAPI.Models;

namespace AntiScamAPI.Data;

public class DatabaseSeeder
{
    private readonly AntiScamDbContext _context;
    private readonly ILogger<DatabaseSeeder> _logger;
    private readonly string _dataPath;

    public DatabaseSeeder(AntiScamDbContext context, ILogger<DatabaseSeeder> logger, string dataPath)
    {
        _context = context;
        _logger = logger;
        _dataPath = dataPath;
    }

    public async Task SeedAsync()
    {
        _logger.LogInformation("Starting database seeding...");

        // Check if already seeded
        var hasModules = await _context.Modules.AnyAsync();
        if (hasModules)
        {
            _logger.LogInformation("Database already seeded. Skipping.");
            return;
        }

        var mlContext = new MLContext(seed: 42);

        // Seed Modules
        await SeedModulesAsync(mlContext);

        // Seed Users
        await SeedUsersAsync(mlContext);

        // Seed Ratings
        await SeedRatingsAsync(mlContext);

        _logger.LogInformation("Database seeding completed successfully!");
    }

    private async Task SeedModulesAsync(MLContext mlContext)
    {
        _logger.LogInformation("Seeding modules...");

        var modulesPath = Path.Combine(_dataPath, "modules.csv");
        if (!File.Exists(modulesPath))
        {
            _logger.LogWarning("modules.csv not found at {Path}", modulesPath);
            return;
        }

        var dataView = mlContext.Data.LoadFromTextFile<ModuleInput>(
            path: modulesPath,
            hasHeader: true,
            separatorChar: ','
        );

        var modules = mlContext.Data.CreateEnumerable<ModuleInput>(dataView, reuseRowObject: false).ToList();

        var entities = modules.Select(m => new ModuleEntity
        {
            ModuleId = m.ModuleId,
            ScamType = m.ScamType,
            Difficulty = m.Difficulty,
            TargetLiteracy = m.TargetLiteracy,
            DurationMin = m.DurationMin
        }).ToList();

        await _context.Modules.AddRangeAsync(entities);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Seeded {Count} modules", entities.Count);
    }

    private async Task SeedUsersAsync(MLContext mlContext)
    {
        _logger.LogInformation("Seeding users...");

        var usersPath = Path.Combine(_dataPath, "users.csv");
        if (!File.Exists(usersPath))
        {
            _logger.LogWarning("users.csv not found at {Path}", usersPath);
            return;
        }

        var dataView = mlContext.Data.LoadFromTextFile<UserInput>(
            path: usersPath,
            hasHeader: true,
            separatorChar: ','
        );

        var users = mlContext.Data.CreateEnumerable<UserInput>(dataView, reuseRowObject: false).ToList();

        var entities = users.Select(u => new UserEntity
        {
            UserId = u.UserId,
            UserCluster = u.UserCluster,
            DigitalLiteracy = u.DigitalLiteracy,
            AgeGroup = u.AgeGroup,
            RiskProfile = u.RiskProfile,
            PreferredTopic = u.PreferredTopic
        }).ToList();

        await _context.Users.AddRangeAsync(entities);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Seeded {Count} users", entities.Count);
    }

    private async Task SeedRatingsAsync(MLContext mlContext)
    {
        _logger.LogInformation("Seeding ratings...");

        var ratingsPath = Path.Combine(_dataPath, "ratings.csv");
        if (!File.Exists(ratingsPath))
        {
            _logger.LogWarning("ratings.csv not found at {Path}", ratingsPath);
            return;
        }

        var dataView = mlContext.Data.LoadFromTextFile<RatingInput>(
            path: ratingsPath,
            hasHeader: true,
            separatorChar: ','
        );

        var ratings = mlContext.Data.CreateEnumerable<RatingInput>(dataView, reuseRowObject: false).ToList();

        // Batch insert for performance
        var entities = ratings.Select(r => new RatingEntity
        {
            UserId = r.UserId,
            ModuleId = r.ModuleId,
            Rating = r.Rating,
            CreatedAt = DateTime.UtcNow
        }).ToList();

        const int batchSize = 1000;
        for (int i = 0; i < entities.Count; i += batchSize)
        {
            var batch = entities.Skip(i).Take(batchSize).ToList();
            await _context.Ratings.AddRangeAsync(batch);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Seeded {Current}/{Total} ratings", Math.Min(i + batchSize, entities.Count), entities.Count);
        }

        _logger.LogInformation("Seeded {Count} ratings total", entities.Count);
    }
}
