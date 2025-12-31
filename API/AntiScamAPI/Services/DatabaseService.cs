using AntiScamAPI.Data;
using AntiScamAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace AntiScamAPI.Services;

public class DatabaseService
{
    private readonly AntiScamDbContext _context;
    private readonly ILogger<DatabaseService> _logger;

    public DatabaseService(AntiScamDbContext context, ILogger<DatabaseService> logger)
    {
        _context = context;
        _logger = logger;
    }

    // USERS
    public async Task<List<UserInput>> LoadUsersAsync()
    {
        var entities = await _context.Users.ToListAsync();
        return entities.Select(e => new UserInput
        {
            UserId = e.UserId,
            UserCluster = e.UserCluster,
            DigitalLiteracy = e.DigitalLiteracy,
            AgeGroup = e.AgeGroup,
            RiskProfile = e.RiskProfile,
            PreferredTopic = e.PreferredTopic
        }).ToList();
    }

    public async Task<UserInput?> GetUserAsync(uint userId)
    {
        var entity = await _context.Users.FindAsync(userId);
        if (entity == null) return null;

        return new UserInput
        {
            UserId = entity.UserId,
            UserCluster = entity.UserCluster,
            DigitalLiteracy = entity.DigitalLiteracy,
            AgeGroup = entity.AgeGroup,
            RiskProfile = entity.RiskProfile,
            PreferredTopic = entity.PreferredTopic
        };
    }

    public async Task<uint> GetNextUserIdAsync()
    {
        var maxId = await _context.Users.MaxAsync(u => (uint?)u.UserId);
        return (maxId ?? 0) + 1;
    }

    public async Task<UserInput> AddUserAsync(UserInput user)
    {
        var entity = new UserEntity
        {
            UserId = user.UserId,
            UserCluster = user.UserCluster,
            DigitalLiteracy = user.DigitalLiteracy,
            AgeGroup = user.AgeGroup,
            RiskProfile = user.RiskProfile,
            PreferredTopic = user.PreferredTopic
        };

        _context.Users.Add(entity);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Added user {UserId} to database", user.UserId);
        return user;
    }

    // MODULES
    public async Task<List<ModuleInput>> LoadModulesAsync()
    {
        var entities = await _context.Modules.ToListAsync();
        return entities.Select(e => new ModuleInput
        {
            ModuleId = e.ModuleId,
            ScamType = e.ScamType,
            Difficulty = e.Difficulty,
            TargetLiteracy = e.TargetLiteracy,
            DurationMin = e.DurationMin
        }).ToList();
    }

    public async Task<ModuleInput?> GetModuleAsync(uint moduleId)
    {
        var entity = await _context.Modules.FindAsync(moduleId);
        if (entity == null) return null;

        return new ModuleInput
        {
            ModuleId = entity.ModuleId,
            ScamType = entity.ScamType,
            Difficulty = entity.Difficulty,
            TargetLiteracy = entity.TargetLiteracy,
            DurationMin = entity.DurationMin
        };
    }

    // RATINGS
    public async Task<List<RatingInput>> LoadRatingsAsync()
    {
        var entities = await _context.Ratings.ToListAsync();
        return entities.Select(e => new RatingInput
        {
            UserId = e.UserId,
            ModuleId = e.ModuleId,
            Rating = e.Rating
        }).ToList();
    }

    public async Task<RatingInput> AddRatingAsync(RatingInput rating)
    {
        var entity = new RatingEntity
        {
            UserId = rating.UserId,
            ModuleId = rating.ModuleId,
            Rating = rating.Rating,
            CreatedAt = DateTime.UtcNow
        };

        _context.Ratings.Add(entity);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Added rating: User {UserId}, Module {ModuleId}, Rating {Rating}",
            rating.UserId, rating.ModuleId, rating.Rating);

        return rating;
    }

    // STATISTICS
    public async Task<(int Users, int Modules, int Ratings)> GetStatisticsAsync()
    {
        var users = await _context.Users.CountAsync();
        var modules = await _context.Modules.CountAsync();
        var ratings = await _context.Ratings.CountAsync();

        return (users, modules, ratings);
    }
}
