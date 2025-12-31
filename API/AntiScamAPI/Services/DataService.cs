using AntiScamAPI.Models;
using Microsoft.ML;
using System.Globalization;

namespace AntiScamAPI.Services;

public class DataService
{
    private readonly string _dataPath;
    private readonly MLContext _mlContext;
    private readonly object _lockObject = new();

    public DataService(string dataPath)
    {
        _dataPath = dataPath;
        _mlContext = new MLContext(seed: 42);
    }

    public List<ModuleInput> LoadModules()
    {
        var modulesPath = Path.Combine(_dataPath, "modules.csv");
        var dataView = _mlContext.Data.LoadFromTextFile<ModuleInput>(
            path: modulesPath,
            hasHeader: true,
            separatorChar: ','
        );
        return _mlContext.Data.CreateEnumerable<ModuleInput>(dataView, reuseRowObject: false).ToList();
    }

    public List<UserInput> LoadUsers()
    {
        var usersPath = Path.Combine(_dataPath, "users.csv");
        var dataView = _mlContext.Data.LoadFromTextFile<UserInput>(
            path: usersPath,
            hasHeader: true,
            separatorChar: ','
        );
        return _mlContext.Data.CreateEnumerable<UserInput>(dataView, reuseRowObject: false).ToList();
    }

    public List<RatingInput> LoadRatings()
    {
        var ratingsPath = Path.Combine(_dataPath, "ratings.csv");
        var dataView = _mlContext.Data.LoadFromTextFile<RatingInput>(
            path: ratingsPath,
            hasHeader: true,
            separatorChar: ','
        );
        return _mlContext.Data.CreateEnumerable<RatingInput>(dataView, reuseRowObject: false).ToList();
    }

    public void AppendRating(RatingInput rating)
    {
        lock (_lockObject)
        {
            var ratingsPath = Path.Combine(_dataPath, "ratings.csv");
            var line = $"{rating.UserId},{rating.ModuleId},{rating.Rating.ToString(CultureInfo.InvariantCulture)}";
            File.AppendAllText(ratingsPath, Environment.NewLine + line);
        }
    }

    public uint GetNextUserId()
    {
        var users = LoadUsers();
        return users.Any() ? users.Max(u => u.UserId) + 1 : 1;
    }

    public void AppendUser(UserInput user)
    {
        lock (_lockObject)
        {
            var usersPath = Path.Combine(_dataPath, "users.csv");
            var line = $"{user.UserId},{user.UserCluster},{user.DigitalLiteracy.ToString(CultureInfo.InvariantCulture)},{user.AgeGroup},{user.RiskProfile},{user.PreferredTopic}";
            File.AppendAllText(usersPath, Environment.NewLine + line);
        }
    }

    public ModuleInput? GetModule(uint moduleId)
    {
        var modules = LoadModules();
        return modules.FirstOrDefault(m => m.ModuleId == moduleId);
    }

    public UserInput? GetUser(uint userId)
    {
        var users = LoadUsers();
        return users.FirstOrDefault(u => u.UserId == userId);
    }
}
