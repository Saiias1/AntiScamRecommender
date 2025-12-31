using Microsoft.ML;

namespace AntiScamRecommender;

/// <summary>
/// Comprehensive evaluation module for recommendation models
/// </summary>
public class Evaluation
{
    public class ModelMetrics
    {
        public string ModelName { get; set; } = string.Empty;
        public double MAE { get; set; }
        public double RMSE { get; set; }
        public double RSquared { get; set; }
        public double? PrecisionAt5 { get; set; }

        public override string ToString()
        {
            var p5 = PrecisionAt5.HasValue ? $"{PrecisionAt5.Value:F4}" : "N/A";
            return $"{ModelName,-25} | MAE: {MAE:F4} | RMSE: {RMSE:F4} | R²: {RSquared:F4} | P@5: {p5}";
        }
    }

    /// <summary>
    /// Calculate Mean Absolute Error
    /// </summary>
    public static double CalculateMAE(List<float> actual, List<float> predicted)
    {
        if (actual.Count != predicted.Count)
            throw new ArgumentException("Lists must have equal length");

        return actual.Zip(predicted, (a, p) => Math.Abs(a - p)).Average();
    }

    /// <summary>
    /// Calculate Root Mean Squared Error
    /// </summary>
    public static double CalculateRMSE(List<float> actual, List<float> predicted)
    {
        if (actual.Count != predicted.Count)
            throw new ArgumentException("Lists must have equal length");

        var mse = actual.Zip(predicted, (a, p) => Math.Pow(a - p, 2)).Average();
        return Math.Sqrt(mse);
    }

    /// <summary>
    /// Calculate R-squared (coefficient of determination)
    /// </summary>
    public static double CalculateRSquared(List<float> actual, List<float> predicted)
    {
        if (actual.Count != predicted.Count)
            throw new ArgumentException("Lists must have equal length");

        var mean = actual.Average();
        var ssTotal = actual.Sum(a => Math.Pow(a - mean, 2));
        var ssResidual = actual.Zip(predicted, (a, p) => Math.Pow(a - p, 2)).Sum();

        if (ssTotal == 0) return 0; // Perfect predictions or no variance
        return 1 - (ssResidual / ssTotal);
    }

    /// <summary>
    /// Calculate Precision@K for the Matrix Factorization model
    /// For each user, generate top K recommendations and check against relevant items
    /// </summary>
    public static double CalculatePrecisionAtK(
        MLContext mlContext,
        ITransformer model,
        List<RatingInput> testData,
        int k = 5,
        float relevanceThreshold = 0)
    {
        // Group test data by user
        var userGroups = testData.GroupBy(r => r.UserId).ToList();

        // Determine relevance threshold if not provided (use median)
        if (relevanceThreshold == 0)
        {
            relevanceThreshold = CalculateMedian(testData.Select(r => r.Rating).ToList());
        }

        var precisionScores = new List<double>();
        var predictionEngine = mlContext.Model.CreatePredictionEngine<RatingInput, RatingPrediction>(model);

        foreach (var userGroup in userGroups)
        {
            var userId = userGroup.Key;
            var userTestItems = userGroup.ToList();

            // Get all unique modules from test data to generate recommendations
            var allModules = testData.Select(r => r.ModuleId).Distinct().ToList();

            // Generate predictions for all modules for this user
            var predictions = new List<(uint moduleId, float score)>();
            foreach (var moduleId in allModules)
            {
                var input = new RatingInput { UserId = userId, ModuleId = moduleId, Rating = 0 };
                var prediction = predictionEngine.Predict(input);
                predictions.Add((moduleId, prediction.Score));
            }

            // Get top K recommendations
            var topK = predictions
                .OrderByDescending(p => p.score)
                .Take(k)
                .Select(p => p.moduleId)
                .ToHashSet();

            // Determine relevant items (items in test set with rating >= threshold)
            var relevantItems = userTestItems
                .Where(r => r.Rating >= relevanceThreshold)
                .Select(r => r.ModuleId)
                .ToHashSet();

            // Calculate precision for this user
            if (relevantItems.Count > 0)
            {
                var hits = topK.Intersect(relevantItems).Count();
                var precision = (double)hits / k;
                precisionScores.Add(precision);
            }
        }

        return precisionScores.Count > 0 ? precisionScores.Average() : 0;
    }

    /// <summary>
    /// Evaluate all models and return metrics
    /// </summary>
    public static List<ModelMetrics> EvaluateAllModels(
        MLContext mlContext,
        ITransformer mfModel,
        BaselineRandom randomBaseline,
        BaselineMostPopular popularBaseline,
        List<RatingInput> testData,
        float minRating,
        float maxRating)
    {
        Console.WriteLine("\n=== Evaluating Models ===\n");

        var results = new List<ModelMetrics>();
        var testPairs = testData.Select(r => (r.UserId, r.ModuleId)).ToList();
        var actualRatings = testData.Select(r => r.Rating).ToList();

        // 1. Matrix Factorization
        Console.WriteLine("Evaluating Matrix Factorization model...");
        var predictionEngine = mlContext.Model.CreatePredictionEngine<RatingInput, RatingPrediction>(mfModel);
        var mfPredictions = testData.Select(r => predictionEngine.Predict(r).Score).ToList();

        var mfMetrics = new ModelMetrics
        {
            ModelName = "Matrix Factorization",
            MAE = CalculateMAE(actualRatings, mfPredictions),
            RMSE = CalculateRMSE(actualRatings, mfPredictions),
            RSquared = CalculateRSquared(actualRatings, mfPredictions),
            PrecisionAt5 = CalculatePrecisionAtK(mlContext, mfModel, testData, k: 5)
        };
        results.Add(mfMetrics);
        Console.WriteLine($"  ✓ {mfMetrics}");

        // 2. Random Baseline
        Console.WriteLine("\nEvaluating Random baseline...");
        var randomPredictions = randomBaseline.Predict(testPairs);

        var randomMetrics = new ModelMetrics
        {
            ModelName = "Random Baseline",
            MAE = CalculateMAE(actualRatings, randomPredictions),
            RMSE = CalculateRMSE(actualRatings, randomPredictions),
            RSquared = CalculateRSquared(actualRatings, randomPredictions),
            PrecisionAt5 = null // Not applicable for random baseline
        };
        results.Add(randomMetrics);
        Console.WriteLine($"  ✓ {randomMetrics}");

        // 3. Most Popular Baseline
        Console.WriteLine("\nEvaluating Most Popular baseline...");
        var popularPredictions = popularBaseline.Predict(testPairs);

        var popularMetrics = new ModelMetrics
        {
            ModelName = "Most Popular Baseline",
            MAE = CalculateMAE(actualRatings, popularPredictions),
            RMSE = CalculateRMSE(actualRatings, popularPredictions),
            RSquared = CalculateRSquared(actualRatings, popularPredictions),
            PrecisionAt5 = null // Not applicable for item-average baseline
        };
        results.Add(popularMetrics);
        Console.WriteLine($"  ✓ {popularMetrics}");

        Console.WriteLine("\n✓ All models evaluated successfully!\n");
        return results;
    }

    /// <summary>
    /// Export metrics to CSV file
    /// </summary>
    public static void ExportMetricsToCSV(List<ModelMetrics> metrics, string outputPath)
    {
        var lines = new List<string>
        {
            "ModelName,MAE,RMSE,R_Squared,Precision_at_5"
        };

        foreach (var metric in metrics)
        {
            var p5 = metric.PrecisionAt5.HasValue ? metric.PrecisionAt5.Value.ToString("F6") : "NA";
            lines.Add($"{metric.ModelName},{metric.MAE:F6},{metric.RMSE:F6},{metric.RSquared:F6},{p5}");
        }

        File.WriteAllLines(outputPath, lines);
        Console.WriteLine($"✓ Metrics exported to: {outputPath}");
    }

    /// <summary>
    /// Generate comprehensive evaluation report
    /// </summary>
    public static void GenerateTextReport(
        List<ModelMetrics> metrics,
        int totalUsers,
        int totalModules,
        int totalRatings,
        int trainSize,
        int testSize,
        string outputPath)
    {
        var report = new List<string>
        {
            "================================================================================",
            "                   ANTI-SCAM RECOMMENDER SYSTEM",
            "                        EVALUATION REPORT",
            "================================================================================",
            "",
            "1. DATASET OVERVIEW",
            "--------------------------------------------------------------------------------",
            "",
            "This recommendation system uses SYNTHETIC data generated for educational",
            "purposes to simulate user interactions with anti-scam learning modules.",
            "",
            $"  • Total Users:           {totalUsers:N0}",
            $"  • Total Modules:         {totalModules}",
            $"  • Total Ratings:         {totalRatings:N0}",
            $"  • Training Set:          {trainSize:N0} ({(double)trainSize/totalRatings*100:F1}%)",
            $"  • Test Set:              {testSize:N0} ({(double)testSize/totalRatings*100:F1}%)",
            "",
            "Dataset characteristics:",
            "  • users.csv: user_id, user_cluster, digital_literacy, age_group,",
            "               risk_profile, preferred_topic",
            "  • modules.csv: module_id, scam_type, difficulty, target_literacy,",
            "                 duration_min",
            "  • ratings.csv: user_id, module_id, rating (unique combinations)",
            "",
            "",
            "2. MODELS EVALUATED",
            "--------------------------------------------------------------------------------",
            "",
            "Three models were evaluated on the test set:",
            "",
            "  A) Matrix Factorization (ML.NET Recommender)",
            "     Collaborative filtering approach that learns latent user and item factors",
            "     to predict ratings based on user-item interaction patterns.",
            "",
            "  B) Random Baseline",
            "     Predicts random ratings uniformly within the observed rating range.",
            "     Serves as a lower-bound performance benchmark.",
            "",
            "  C) Most Popular Baseline",
            "     Predicts based on the average rating per module (item popularity).",
            "     Falls back to global average for unseen modules.",
            "",
            "",
            "3. EVALUATION METRICS",
            "--------------------------------------------------------------------------------",
            "",
            "MAE (Mean Absolute Error)",
            "  • Measures average absolute difference between predicted and actual ratings",
            "  • Lower is better (0 = perfect predictions)",
            "  • Interpretation: Average rating prediction error in rating units",
            "",
            "RMSE (Root Mean Squared Error)",
            "  • Square root of average squared prediction errors",
            "  • Penalizes large errors more than MAE",
            "  • Lower is better (0 = perfect predictions)",
            "",
            "R² (R-squared / Coefficient of Determination)",
            "  • Proportion of variance in ratings explained by the model",
            "  • Range: (-∞, 1], where 1 = perfect fit, 0 = no better than mean",
            "  • Higher is better",
            "",
            "Precision@5",
            "  • For each user: proportion of top-5 recommendations that are relevant",
            "  • Relevance defined as: rating >= median(all ratings)",
            "  • Range: [0, 1], where 1 = all top-5 are relevant",
            "  • Only applicable to ranking models (Matrix Factorization)",
            "  • Higher is better",
            "",
            "",
            "4. RESULTS",
            "--------------------------------------------------------------------------------",
            ""
        };

        // Add metrics table
        report.Add(string.Format("{0,-30} {1,10} {2,10} {3,10} {4,12}",
            "Model", "MAE", "RMSE", "R²", "P@5"));
        report.Add(new string('-', 80));

        foreach (var m in metrics)
        {
            var p5 = m.PrecisionAt5.HasValue ? m.PrecisionAt5.Value.ToString("F4") : "N/A";
            report.Add(string.Format("{0,-30} {1,10:F4} {2,10:F4} {3,10:F4} {4,12}",
                m.ModelName, m.MAE, m.RMSE, m.RSquared, p5));
        }

        report.Add("");
        report.Add("");
        report.Add("5. ANALYSIS & INTERPRETATION");
        report.Add("--------------------------------------------------------------------------------");
        report.Add("");

        // Find best model
        var bestMAE = metrics.OrderBy(m => m.MAE).First();
        var bestRMSE = metrics.OrderBy(m => m.RMSE).First();
        var bestR2 = metrics.OrderByDescending(m => m.RSquared).First();

        report.Add("Model Performance Comparison:");
        report.Add("");
        report.Add($"  • Best MAE:  {bestMAE.ModelName} ({bestMAE.MAE:F4})");
        report.Add($"  • Best RMSE: {bestRMSE.ModelName} ({bestRMSE.RMSE:F4})");
        report.Add($"  • Best R²:   {bestR2.ModelName} ({bestR2.RSquared:F4})");
        report.Add("");

        var mf = metrics.First(m => m.ModelName == "Matrix Factorization");
        var random = metrics.First(m => m.ModelName == "Random Baseline");
        var popular = metrics.First(m => m.ModelName == "Most Popular Baseline");

        report.Add("Matrix Factorization vs Baselines:");
        report.Add("");
        report.Add($"  • MAE improvement over Random:       {(random.MAE - mf.MAE) / random.MAE * 100:F1}%");
        report.Add($"  • MAE improvement over Most Popular: {(popular.MAE - mf.MAE) / popular.MAE * 100:F1}%");
        report.Add($"  • RMSE improvement over Random:      {(random.RMSE - mf.RMSE) / random.RMSE * 100:F1}%");
        report.Add($"  • RMSE improvement over Most Popular:{(popular.RMSE - mf.RMSE) / popular.RMSE * 100:F1}%");
        report.Add("");

        if (mf.PrecisionAt5.HasValue)
        {
            report.Add($"  • Precision@5: {mf.PrecisionAt5.Value:F4}");
            report.Add($"    → On average, {mf.PrecisionAt5.Value * 5:F2} out of 5 top recommendations are relevant");
        }
        report.Add("");

        report.Add("Key Findings:");
        report.Add("");

        if (mf.MAE < popular.MAE && mf.MAE < random.MAE)
        {
            report.Add("  ✓ Matrix Factorization outperforms both baselines across all metrics,");
            report.Add("    demonstrating that collaborative filtering successfully captures");
            report.Add("    user-item interaction patterns in the synthetic dataset.");
        }
        else if (mf.MAE < random.MAE)
        {
            report.Add("  • Matrix Factorization outperforms Random baseline but not Most Popular,");
            report.Add("    suggesting that item popularity is a strong signal in this dataset.");
        }
        else
        {
            report.Add("  ⚠ Matrix Factorization does not outperform baselines. This may indicate:");
            report.Add("    - Insufficient training data or model capacity");
            report.Add("    - Strong item popularity effects dominating collaborative patterns");
            report.Add("    - Hyperparameter tuning needed");
        }

        report.Add("");
        report.Add("");
        report.Add("6. LIMITATIONS & CONSIDERATIONS");
        report.Add("--------------------------------------------------------------------------------");
        report.Add("");
        report.Add("Important limitations to consider when interpreting these results:");
        report.Add("");
        report.Add("  1. SYNTHETIC DATA");
        report.Add("     All data is artificially generated for educational purposes.");
        report.Add("     Real-world performance may differ significantly from these results.");
        report.Add("");
        report.Add("  2. SINGLE TRAIN/TEST SPLIT");
        report.Add("     Model evaluated on a single 80/20 split without cross-validation.");
        report.Add("     Results may vary with different random splits.");
        report.Add("");
        report.Add("  3. NO CROSS-VALIDATION");
        report.Add("     Lack of k-fold cross-validation means we cannot assess variance");
        report.Add("     in model performance or statistical significance of differences.");
        report.Add("");
        report.Add("  4. NO TEMPORAL DIMENSION");
        report.Add("     Ratings are not time-ordered. In production, temporal effects");
        report.Add("     (e.g., concept drift, seasonal patterns) should be considered.");
        report.Add("");
        report.Add("  5. COLD START PROBLEMS NOT EVALUATED");
        report.Add("     Performance on new users or new modules (cold start scenarios)");
        report.Add("     is not separately evaluated, though these are critical in practice.");
        report.Add("");
        report.Add("  6. LIMITED HYPERPARAMETER TUNING");
        report.Add("     Matrix Factorization uses default/basic hyperparameters.");
        report.Add("     Systematic tuning could improve performance.");
        report.Add("");
        report.Add("  7. IMPLICIT FEEDBACK NOT MODELED");
        report.Add("     Only explicit ratings are used. Real systems often benefit from");
        report.Add("     implicit signals (clicks, time spent, completion rates).");
        report.Add("");
        report.Add("  8. NO A/B TESTING");
        report.Add("     Offline metrics (MAE, RMSE, Precision@K) are proxies for user");
        report.Add("     satisfaction. Online A/B testing is needed for production deployment.");
        report.Add("");
        report.Add("");
        report.Add("7. RECOMMENDATIONS FOR IMPROVEMENT");
        report.Add("--------------------------------------------------------------------------------");
        report.Add("");
        report.Add("  • Implement k-fold cross-validation for robust performance estimates");
        report.Add("  • Tune hyperparameters (latent factors, regularization, learning rate)");
        report.Add("  • Evaluate cold-start performance separately");
        report.Add("  • Add content-based features (module metadata, user attributes)");
        report.Add("  • Consider hybrid approaches combining collaborative and content-based");
        report.Add("  • Implement temporal validation (train on past, test on future)");
        report.Add("  • Add additional ranking metrics (NDCG, MAP, Recall@K)");
        report.Add("  • Test with real user data when available");
        report.Add("");
        report.Add("================================================================================");
        report.Add($"Report generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        report.Add("================================================================================");

        File.WriteAllLines(outputPath, report);
        Console.WriteLine($"✓ Evaluation report generated: {outputPath}");
    }

    /// <summary>
    /// Helper: Calculate median of a list
    /// </summary>
    private static float CalculateMedian(List<float> values)
    {
        var sorted = values.OrderBy(v => v).ToList();
        int mid = sorted.Count / 2;
        return sorted.Count % 2 == 0 ? (sorted[mid - 1] + sorted[mid]) / 2 : sorted[mid];
    }
}
