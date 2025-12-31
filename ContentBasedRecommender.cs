namespace AntiScamRecommender;

public class ContentBasedRecommender
{
    private readonly List<ModuleInput> _modules;
    private readonly List<UserInput> _users;

    public ContentBasedRecommender(List<ModuleInput> modules, List<UserInput> users)
    {
        _modules = modules;
        _users = users;
    }

    /// <summary>
    /// Calculates content-based score between a user and a module.
    /// Score is between 0 and 1, where 1 is perfect match.
    /// </summary>
    public float PredictScore(uint userId, uint moduleId)
    {
        var user = _users.FirstOrDefault(u => u.UserId == userId);
        var module = _modules.FirstOrDefault(m => m.ModuleId == moduleId);

        if (user == null || module == null)
            return 2.5f; // Default middle rating if user/module not found

        float score = 0f;
        int factors = 0;

        // 1. Topic match (40% weight) - most important
        if (user.PreferredTopic.Equals(module.ScamType, StringComparison.OrdinalIgnoreCase))
        {
            score += 0.4f;
        }
        factors++;

        // 2. Literacy match (30% weight) - should match user's literacy level
        float literacyDiff = Math.Abs(user.DigitalLiteracy - module.TargetLiteracy);
        float literacyScore = Math.Max(0, 1 - (literacyDiff / 5.0f)); // Normalize to 0-1
        score += literacyScore * 0.3f;
        factors++;

        // 3. Difficulty appropriateness (20% weight)
        // Users with higher literacy can handle higher difficulty
        float difficultyScore;
        if (module.Difficulty <= user.DigitalLiteracy)
        {
            // Module difficulty is appropriate or easier
            difficultyScore = 1.0f - ((user.DigitalLiteracy - module.Difficulty) / 5.0f) * 0.3f;
        }
        else
        {
            // Module is too difficult - penalize more
            float diffGap = module.Difficulty - user.DigitalLiteracy;
            difficultyScore = Math.Max(0, 1 - (diffGap / 5.0f) * 1.5f);
        }
        score += difficultyScore * 0.2f;
        factors++;

        // 4. Duration preference (10% weight)
        // Shorter modules (3-7 min) are generally preferred, longer ones for engaged users
        float durationScore = module.DurationMin <= 7 ? 1.0f : 0.7f;
        score += durationScore * 0.1f;
        factors++;

        // Convert 0-1 score to 1-5 rating scale
        float rating = 1.0f + (score * 4.0f);
        return Math.Clamp(rating, 1.0f, 5.0f);
    }

    /// <summary>
    /// Get top N module recommendations for a user based on content similarity.
    /// </summary>
    public List<(uint moduleId, float score)> GetTopRecommendations(uint userId, int topN = 5)
    {
        var predictions = _modules
            .Select(m => (moduleId: m.ModuleId, score: PredictScore(userId, m.ModuleId)))
            .OrderByDescending(x => x.score)
            .Take(topN)
            .ToList();

        return predictions;
    }
}
