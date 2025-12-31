namespace AntiScamRecommender;

/// <summary>
/// Random baseline that predicts ratings uniformly within the actual rating range
/// </summary>
public class BaselineRandom
{
    private readonly Random _random;
    private readonly float _minRating;
    private readonly float _maxRating;

    public BaselineRandom(float minRating, float maxRating, int seed = 42)
    {
        _random = new Random(seed);
        _minRating = minRating;
        _maxRating = maxRating;
    }

    /// <summary>
    /// Predict a random rating within the observed range
    /// </summary>
    public float Predict(uint userId, uint moduleId)
    {
        return _minRating + (float)_random.NextDouble() * (_maxRating - _minRating);
    }

    /// <summary>
    /// Generate predictions for a list of user-item pairs
    /// </summary>
    public List<float> Predict(List<(uint userId, uint moduleId)> pairs)
    {
        return pairs.Select(p => Predict(p.userId, p.moduleId)).ToList();
    }

    public string GetModelName() => "Random Baseline";
}
