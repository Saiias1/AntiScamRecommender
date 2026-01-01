using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AntiScamRecommender
{
    /// <summary>
    /// Hyperparameter tuning for Matrix Factorization recommender
    /// Grid search over: ApproximationRank, LearningRate, NumberOfIterations
    /// </summary>
    public class HyperparameterTuning
    {
        private readonly MLContext _mlContext;
        private readonly string _dataPath;

        public HyperparameterTuning(string dataPath = "data/ratings.csv")
        {
            _mlContext = new MLContext(seed: 42);
            _dataPath = dataPath;
        }

        public class TuningResult
        {
            public int ApproximationRank { get; set; }
            public double LearningRate { get; set; }
            public int Iterations { get; set; }
            public double MAE { get; set; }
            public double RMSE { get; set; }
            public double RSquared { get; set; }
            public TimeSpan TrainingTime { get; set; }
        }

        /// <summary>
        /// Grid search for optimal hyperparameters
        /// </summary>
        public List<TuningResult> GridSearch()
        {
            Console.WriteLine("=".PadRight(80, '='));
            Console.WriteLine("HYPERPARAMETER TUNING: MATRIX FACTORIZATION");
            Console.WriteLine("=".PadRight(80, '='));

            // Define hyperparameter grid
            var approximationRanks = new[] { 8, 16, 32, 64 };
            var learningRates = new[] { 0.001, 0.01, 0.1 };
            var iterationsList = new[] { 10, 20, 50 };

            // Load data
            var data = _mlContext.Data.LoadFromTextFile<RatingInput>(
                _dataPath,
                separatorChar: ',',
                hasHeader: true
            );

            // 80/20 train/test split
            var trainTestSplit = _mlContext.Data.TrainTestSplit(data, testFraction: 0.2, seed: 42);
            var trainData = trainTestSplit.TrainSet;
            var testData = trainTestSplit.TestSet;

            var results = new List<TuningResult>();
            int totalCombinations = approximationRanks.Length * learningRates.Length * iterationsList.Length;
            int currentCombination = 0;

            Console.WriteLine($"\nGrid Search Configuration:");
            Console.WriteLine($"  Approximation Ranks: {string.Join(", ", approximationRanks)}");
            Console.WriteLine($"  Learning Rates: {string.Join(", ", learningRates)}");
            Console.WriteLine($"  Iterations: {string.Join(", ", iterationsList)}");
            Console.WriteLine($"\nTotal combinations to test: {totalCombinations}\n");

            // Grid search
            foreach (var rank in approximationRanks)
            {
                foreach (var lr in learningRates)
                {
                    foreach (var iters in iterationsList)
                    {
                        currentCombination++;
                        Console.WriteLine($"[{currentCombination}/{totalCombinations}] Testing: Rank={rank}, LR={lr}, Iters={iters}");

                        var startTime = DateTime.Now;

                        // Train model with specific hyperparameters
                        var pipeline = _mlContext.Transforms.Conversion.MapValueToKey(
                            outputColumnName: "userIdEncoded",
                            inputColumnName: nameof(RatingInput.UserId))
                        .Append(_mlContext.Transforms.Conversion.MapValueToKey(
                            outputColumnName: "moduleIdEncoded",
                            inputColumnName: nameof(RatingInput.ModuleId)))
                        .Append(_mlContext.Recommendation().Trainers.MatrixFactorization(
                            labelColumnName: nameof(RatingInput.Rating),
                            matrixColumnIndexColumnName: "userIdEncoded",
                            matrixRowIndexColumnName: "moduleIdEncoded",
                            approximationRank: rank,
                            learningRate: lr,
                            numberOfIterations: iters
                        ));

                        var model = pipeline.Fit(trainData);
                        var trainingTime = DateTime.Now - startTime;

                        // Evaluate
                        var predictions = model.Transform(testData);
                        var metrics = _mlContext.Regression.Evaluate(
                            predictions,
                            labelColumnName: nameof(RatingInput.Rating),
                            scoreColumnName: "Score"
                        );

                        results.Add(new TuningResult
                        {
                            ApproximationRank = rank,
                            LearningRate = lr,
                            Iterations = iters,
                            MAE = metrics.MeanAbsoluteError,
                            RMSE = metrics.RootMeanSquaredError,
                            RSquared = metrics.RSquared,
                            TrainingTime = trainingTime
                        });

                        Console.WriteLine($"    → MAE: {metrics.MeanAbsoluteError:F4}, RMSE: {metrics.RootMeanSquaredError:F4}, R²: {metrics.RSquared:F4}, Time: {trainingTime.TotalSeconds:F1}s\n");
                    }
                }
            }

            return results;
        }

        /// <summary>
        /// Analyze and report best hyperparameters
        /// </summary>
        public void AnalyzeResults(List<TuningResult> results)
        {
            Console.WriteLine("\n" + "=".PadRight(80, '='));
            Console.WriteLine("HYPERPARAMETER TUNING RESULTS");
            Console.WriteLine("=".PadRight(80, '='));

            // Sort by MAE (best = lowest)
            var sortedByMAE = results.OrderBy(r => r.MAE).ToList();

            Console.WriteLine("\n🏆 TOP 5 CONFIGURATIONS (by MAE):");
            Console.WriteLine("-".PadRight(80, '-'));
            Console.WriteLine($"{"Rank",-6} {"LR",-8} {"Iters",-8} {"MAE",-10} {"RMSE",-10} {"R²",-10} {"Time (s)",-10}");
            Console.WriteLine("-".PadRight(80, '-'));

            for (int i = 0; i < Math.Min(5, sortedByMAE.Count); i++)
            {
                var r = sortedByMAE[i];
                Console.WriteLine($"{r.ApproximationRank,-6} {r.LearningRate,-8:F4} {r.Iterations,-8} {r.MAE,-10:F4} {r.RMSE,-10:F4} {r.RSquared,-10:F4} {r.TrainingTime.TotalSeconds,-10:F1}");
            }

            // Best configuration
            var best = sortedByMAE.First();
            Console.WriteLine("\n✅ OPTIMAL HYPERPARAMETERS:");
            Console.WriteLine($"  Approximation Rank: {best.ApproximationRank}");
            Console.WriteLine($"  Learning Rate: {best.LearningRate:F4}");
            Console.WriteLine($"  Iterations: {best.Iterations}");
            Console.WriteLine($"\n  Performance:");
            Console.WriteLine($"    MAE: {best.MAE:F4}");
            Console.WriteLine($"    RMSE: {best.RMSE:F4}");
            Console.WriteLine($"    R²: {best.RSquared:F4}");
            Console.WriteLine($"    Training Time: {best.TrainingTime.TotalSeconds:F2}s");

            // Worst configuration (for comparison)
            var worst = sortedByMAE.Last();
            double improvement = ((worst.MAE - best.MAE) / worst.MAE) * 100;
            Console.WriteLine($"\n  Improvement over worst config: {improvement:F1}%");

            // Save results to CSV
            SaveResultsToCsv(results, "evaluation/hyperparameter_tuning_results.csv");
            Console.WriteLine($"\n✓ Full results saved to: evaluation/hyperparameter_tuning_results.csv");
        }

        /// <summary>
        /// Save tuning results to CSV
        /// </summary>
        private void SaveResultsToCsv(List<TuningResult> results, string filePath)
        {
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            using (var writer = new StreamWriter(filePath))
            {
                // Header
                writer.WriteLine("approximation_rank,learning_rate,iterations,mae,rmse,r_squared,training_time_seconds");

                // Data
                foreach (var result in results.OrderBy(r => r.MAE))
                {
                    writer.WriteLine($"{result.ApproximationRank},{result.LearningRate},{result.Iterations},{result.MAE:F6},{result.RMSE:F6},{result.RSquared:F6},{result.TrainingTime.TotalSeconds:F2}");
                }
            }
        }

        /// <summary>
        /// Analyze impact of each hyperparameter
        /// </summary>
        public void AnalyzeHyperparameterImpact(List<TuningResult> results)
        {
            Console.WriteLine("\n" + "=".PadRight(80, '='));
            Console.WriteLine("HYPERPARAMETER IMPACT ANALYSIS");
            Console.WriteLine("=".PadRight(80, '='));

            // Group by each hyperparameter and calculate average MAE
            var byRank = results.GroupBy(r => r.ApproximationRank)
                .Select(g => new { Param = g.Key, AvgMAE = g.Average(r => r.MAE) })
                .OrderBy(x => x.AvgMAE);

            var byLR = results.GroupBy(r => r.LearningRate)
                .Select(g => new { Param = g.Key, AvgMAE = g.Average(r => r.MAE) })
                .OrderBy(x => x.AvgMAE);

            var byIters = results.GroupBy(r => r.Iterations)
                .Select(g => new { Param = g.Key, AvgMAE = g.Average(r => r.MAE) })
                .OrderBy(x => x.AvgMAE);

            Console.WriteLine("\nApproximation Rank Impact (Average MAE):");
            Console.WriteLine("-".PadRight(40, '-'));
            foreach (var item in byRank)
            {
                Console.WriteLine($"  Rank {item.Param,3}: {item.AvgMAE:F4}");
            }

            Console.WriteLine("\nLearning Rate Impact (Average MAE):");
            Console.WriteLine("-".PadRight(40, '-'));
            foreach (var item in byLR)
            {
                Console.WriteLine($"  LR {item.Param,6:F4}: {item.AvgMAE:F4}");
            }

            Console.WriteLine("\nIterations Impact (Average MAE):");
            Console.WriteLine("-".PadRight(40, '-'));
            foreach (var item in byIters)
            {
                Console.WriteLine($"  Iters {item.Param,3}: {item.AvgMAE:F4}");
            }

            Console.WriteLine("\n📊 Key Insights:");
            Console.WriteLine($"  • Best Approximation Rank: {byRank.First().Param}");
            Console.WriteLine($"  • Best Learning Rate: {byLR.First().Param:F4}");
            Console.WriteLine($"  • Best Iterations: {byIters.First().Param}");
        }

        /// <summary>
        /// Main execution
        /// </summary>
        public static void RunTuning()
        {
            var tuner = new HyperparameterTuning();
            var results = tuner.GridSearch();
            tuner.AnalyzeResults(results);
            tuner.AnalyzeHyperparameterImpact(results);

            Console.WriteLine("\n" + "=".PadRight(80, '='));
            Console.WriteLine("HYPERPARAMETER TUNING COMPLETE");
            Console.WriteLine("=".PadRight(80, '='));
        }
    }
}
