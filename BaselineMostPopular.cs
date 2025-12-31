namespace AntiScamRecommender;

/// <summary>
/// Most Popular baseline that predicts based on average rating per module (item popularity)
/// Falls back to global average for unseen modules
/// </summary>
public class BaselineMostPopular
{
    private readonly Dictionary<uint, float> _moduleAverages;
    private readonly float _globalAverage;

    public BaselineMostPopular(List<RatingInput> trainingData)
    {
        // Calculate average rating per module
        _moduleAverages = trainingData
            .GroupBy(r => r.ModuleId)
            .ToDictionary(
                g => g.Key,
                g => (float)g.Average(r => r.Rating)
            );

        // Calculate global average as fallback
        _globalAverage = trainingData.Average(r => r.Rating);
    }

    /// <summary>
    /// Predict rating based on module's average rating, or global average if unseen
    /// </summary>
    public float Predict(uint userId, uint moduleId)
    {
        return _moduleAverages.TryGetValue(moduleId, out var avg) ? avg : _globalAverage;
    }

    /// <summary>
    /// Generate predictions for a list of user-item pairs
    /// </summary>
    public List<float> Predict(List<(uint userId, uint moduleId)> pairs)
    {
        return pairs.Select(p => Predict(p.userId, p.moduleId)).ToList();
    }

    public string GetModelName() => "Most Popular Baseline";
    public int KnownModulesCount => _moduleAverages.Count;
    public float GlobalAverage => _globalAverage;
}
