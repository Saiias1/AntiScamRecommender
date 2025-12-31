using Microsoft.ML;

namespace AntiScamAPI.Models;

public class HybridRecommender
{
    private readonly PredictionEngine<RatingInput, RatingPrediction> _collaborativeEngine;
    private readonly ContentBasedRecommender _contentBased;
    private readonly float _collaborativeWeight;
    private readonly float _contentWeight;

    /// <summary>
    /// Creates a hybrid recommender that combines collaborative filtering and content-based filtering.
    /// </summary>
    /// <param name="collaborativeEngine">ML.NET prediction engine for collaborative filtering</param>
    /// <param name="contentBased">Content-based recommender</param>
    /// <param name="collaborativeWeight">Weight for collaborative filtering (0-1). Default: 0.7</param>
    public HybridRecommender(
        PredictionEngine<RatingInput, RatingPrediction> collaborativeEngine,
        ContentBasedRecommender contentBased,
        float collaborativeWeight = 0.7f)
    {
        _collaborativeEngine = collaborativeEngine;
        _contentBased = contentBased;
        _collaborativeWeight = Math.Clamp(collaborativeWeight, 0f, 1f);
        _contentWeight = 1f - _collaborativeWeight;
    }

    /// <summary>
    /// Predicts rating using hybrid approach (collaborative + content-based).
    /// </summary>
    public float Predict(uint userId, uint moduleId)
    {
        // Get collaborative filtering prediction
        var collabPrediction = _collaborativeEngine.Predict(new RatingInput
        {
            UserId = userId,
            ModuleId = moduleId
        });
        float collabScore = collabPrediction.Score;

        // Get content-based prediction
        float contentScore = _contentBased.PredictScore(userId, moduleId);

        // Weighted combination
        float hybridScore = (_collaborativeWeight * collabScore) + (_contentWeight * contentScore);

        return Math.Clamp(hybridScore, 1.0f, 5.0f);
    }

    /// <summary>
    /// Get top N recommendations for a user using hybrid approach.
    /// </summary>
    public List<(uint moduleId, float score)> GetTopRecommendations(
        uint userId,
        List<uint> allModuleIds,
        int topN = 5,
        HashSet<uint>? excludeModules = null)
    {
        excludeModules ??= new HashSet<uint>();

        var predictions = allModuleIds
            .Where(mid => !excludeModules.Contains(mid))
            .Select(mid => (moduleId: mid, score: Predict(userId, mid)))
            .OrderByDescending(x => x.score)
            .Take(topN)
            .ToList();

        return predictions;
    }

    /// <summary>
    /// Get recommendations with separate scores for analysis.
    /// </summary>
    public List<(uint moduleId, float hybridScore, float collabScore, float contentScore)>
        GetDetailedRecommendations(uint userId, List<uint> allModuleIds, int topN = 5)
    {
        var predictions = allModuleIds
            .Select(mid =>
            {
                var collabPred = _collaborativeEngine.Predict(new RatingInput
                {
                    UserId = userId,
                    ModuleId = mid
                });
                float collabScore = collabPred.Score;
                float contentScore = _contentBased.PredictScore(userId, mid);
                float hybridScore = (_collaborativeWeight * collabScore) + (_contentWeight * contentScore);

                return (moduleId: mid, hybridScore, collabScore, contentScore);
            })
            .OrderByDescending(x => x.hybridScore)
            .Take(topN)
            .ToList();

        return predictions;
    }
}
