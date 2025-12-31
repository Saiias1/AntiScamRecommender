using AntiScamAPI.Models;
using Microsoft.ML;

namespace AntiScamAPI.Services;

public class HybridRecommendationService
{
    private readonly HybridRecommender _hybridRecommender;
    private readonly ContentBasedRecommender _contentBasedRecommender;
    private readonly DataService _dataService;
    private readonly List<uint> _allModuleIds;
    private readonly List<ModuleInput> _modules;
    private readonly Dictionary<uint, UserInput> _usersCache;

    public HybridRecommendationService(string modelPath, string dataPath)
    {
        _dataService = new DataService(dataPath);

        // Load ML.NET model
        var mlContext = new MLContext(seed: 42);
        ITransformer model = mlContext.Model.Load(modelPath, out var _);
        var predictionEngine = mlContext.Model.CreatePredictionEngine<RatingInput, RatingPrediction>(model);

        // Load modules and users for content-based filtering
        _modules = _dataService.LoadModules();
        var users = _dataService.LoadUsers();

        // Cache users for quick lookup
        _usersCache = users.ToDictionary(u => u.UserId);

        _allModuleIds = _modules.Select(m => m.ModuleId).ToList();

        // Create content-based recommender
        _contentBasedRecommender = new ContentBasedRecommender(_modules, users);

        // Create hybrid recommender (70% collaborative + 30% content-based)
        _hybridRecommender = new HybridRecommender(predictionEngine, _contentBasedRecommender, collaborativeWeight: 0.7f);
    }

    public void AddUser(UserInput user)
    {
        _usersCache[user.UserId] = user;
        // Update content-based recommender with new user list
        var allUsers = _usersCache.Values.ToList();
        _contentBasedRecommender.UpdateUsers(allUsers);
    }

    public UserInput? GetCachedUser(uint userId)
    {
        return _usersCache.TryGetValue(userId, out var user) ? user : null;
    }

    public List<ModuleInput> GetModules() => _modules;

    public List<(uint moduleId, float hybridScore, float collabScore, float contentScore)>
        GetRecommendations(uint userId, int topN = 5, bool contentBasedOnly = false)
    {
        if (contentBasedOnly)
        {
            // For new users without ratings, use content-based only
            var contentRecs = _modules
                .Select(m => {
                    var contentScore = _contentBasedRecommender.PredictScore(userId, m.ModuleId);
                    return (moduleId: m.ModuleId, hybridScore: contentScore, collabScore: 0f, contentScore);
                })
                .OrderByDescending(x => x.hybridScore)
                .Take(topN)
                .ToList();

            return contentRecs;
        }

        try
        {
            return _hybridRecommender.GetDetailedRecommendations(userId, _allModuleIds, topN);
        }
        catch (Exception)
        {
            // If collaborative fails (e.g., new user), fall back to content-based
            return GetRecommendations(userId, topN, contentBasedOnly: true);
        }
    }

    public DataService GetDataService() => _dataService;
}
