using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers;
using AntiScamRecommender;

Console.WriteLine("================================================================================");
Console.WriteLine("              ANTI-SCAM RECOMMENDER SYSTEM - ML.NET");
Console.WriteLine("================================================================================");
Console.WriteLine();

// Initialize ML.NET context
var mlContext = new MLContext(seed: 42);

// ============================================================================
// 1. LOAD DATA
// ============================================================================
Console.WriteLine("=== Loading Data ===\n");

// Generate sample data if data folder doesn't exist or is empty
if (!SampleDataGenerator.DataExists("data"))
{
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine("⚠ No data found in 'data/' folder.");
    Console.WriteLine("\nWould you like to generate sample data for testing? (y/n)");
    Console.ResetColor();

    var response = Console.ReadLine()?.Trim().ToLower();
    if (response == "y" || response == "yes")
    {
        SampleDataGenerator.GenerateSampleData("data");
    }
    else
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("\nPlease ensure the following files exist in the 'data/' folder:");
        Console.WriteLine("  • users.csv");
        Console.WriteLine("  • modules.csv");
        Console.WriteLine("  • ratings.csv");
        Console.ResetColor();
        return;
    }
}

var dataPath = Path.Combine("data", "ratings.csv");
var modulesPath = Path.Combine("data", "modules.csv");
var usersPath = Path.Combine("data", "users.csv");

IDataView dataView;
List<ModuleInput> modules;
List<UserInput> users;

try
{
    dataView = mlContext.Data.LoadFromTextFile<RatingInput>(
        path: dataPath,
        hasHeader: true,
        separatorChar: ','
    );
    Console.WriteLine($"✓ Loaded ratings from: {dataPath}");

    // Load modules for content-based filtering
    var modulesDataView = mlContext.Data.LoadFromTextFile<ModuleInput>(
        path: modulesPath,
        hasHeader: true,
        separatorChar: ','
    );
    modules = mlContext.Data.CreateEnumerable<ModuleInput>(modulesDataView, reuseRowObject: false).ToList();
    Console.WriteLine($"✓ Loaded modules from: {modulesPath}");

    // Load users for content-based filtering
    var usersDataView = mlContext.Data.LoadFromTextFile<UserInput>(
        path: usersPath,
        hasHeader: true,
        separatorChar: ','
    );
    users = mlContext.Data.CreateEnumerable<UserInput>(usersDataView, reuseRowObject: false).ToList();
    Console.WriteLine($"✓ Loaded users from: {usersPath}");
}
catch (Exception ex)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"ERROR loading data: {ex.Message}");
    Console.ResetColor();
    return;
}

// Get data statistics
var allRatings = mlContext.Data.CreateEnumerable<RatingInput>(dataView, reuseRowObject: false).ToList();
var totalRatings = allRatings.Count;
var uniqueUsers = allRatings.Select(r => r.UserId).Distinct().Count();
var uniqueModules = allRatings.Select(r => r.ModuleId).Distinct().Count();
var minRating = allRatings.Min(r => r.Rating);
var maxRating = allRatings.Max(r => r.Rating);
var avgRating = allRatings.Average(r => r.Rating);

Console.WriteLine($"\nDataset Statistics:");
Console.WriteLine($"  • Total ratings:     {totalRatings:N0}");
Console.WriteLine($"  • Unique users:      {uniqueUsers:N0}");
Console.WriteLine($"  • Unique modules:    {uniqueModules}");
Console.WriteLine($"  • Rating range:      [{minRating:F1}, {maxRating:F1}]");
Console.WriteLine($"  • Average rating:    {avgRating:F2}");
Console.WriteLine($"  • Sparsity:          {(1.0 - (double)totalRatings / (uniqueUsers * uniqueModules)) * 100:F2}%");

// ============================================================================
// 2. SPLIT DATA (80% train, 20% test)
// ============================================================================
Console.WriteLine("\n=== Splitting Data ===\n");

var trainTestSplit = mlContext.Data.TrainTestSplit(dataView, testFraction: 0.2, seed: 42);
var trainData = trainTestSplit.TrainSet;
var testData = trainTestSplit.TestSet;

var trainList = mlContext.Data.CreateEnumerable<RatingInput>(trainData, reuseRowObject: false).ToList();
var testList = mlContext.Data.CreateEnumerable<RatingInput>(testData, reuseRowObject: false).ToList();

Console.WriteLine($"✓ Training set:   {trainList.Count:N0} ratings ({(double)trainList.Count / totalRatings * 100:F1}%)");
Console.WriteLine($"✓ Test set:       {testList.Count:N0} ratings ({(double)testList.Count / totalRatings * 100:F1}%)");

// ============================================================================
// 3. TRAIN MATRIX FACTORIZATION MODEL
// ============================================================================
Console.WriteLine("\n=== Training Matrix Factorization Model ===\n");

// Convert UserId and ModuleId to key types (required by Matrix Factorization)
var dataProcessPipeline = mlContext.Transforms.Conversion.MapValueToKey(
        outputColumnName: "UserIdKey",
        inputColumnName: nameof(RatingInput.UserId))
    .Append(mlContext.Transforms.Conversion.MapValueToKey(
        outputColumnName: "ModuleIdKey",
        inputColumnName: nameof(RatingInput.ModuleId)));

var options = new MatrixFactorizationTrainer.Options
{
    MatrixColumnIndexColumnName = "UserIdKey",
    MatrixRowIndexColumnName = "ModuleIdKey",
    LabelColumnName = nameof(RatingInput.Rating),
    NumberOfIterations = 20,
    ApproximationRank = 100,
    LearningRate = 0.1,
    Quiet = false
};

var pipeline = dataProcessPipeline.Append(mlContext.Recommendation().Trainers.MatrixFactorization(options));

Console.WriteLine("Training parameters:");
Console.WriteLine($"  • Iterations:        {options.NumberOfIterations}");
Console.WriteLine($"  • Latent factors:    {options.ApproximationRank}");
Console.WriteLine($"  • Learning rate:     {options.LearningRate}");
Console.WriteLine();

ITransformer model;
try
{
    Console.WriteLine("Training in progress...");
    model = pipeline.Fit(trainData);
    Console.WriteLine("\n✓ Matrix Factorization model trained successfully!");
}
catch (Exception ex)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"\nERROR training model: {ex.Message}");
    Console.ResetColor();
    return;
}

// ============================================================================
// 4. SAVE MODEL
// ============================================================================
Console.WriteLine("\n=== Saving Model ===\n");

var modelPath = Path.Combine("model", "model.zip");
Directory.CreateDirectory("model");

try
{
    mlContext.Model.Save(model, trainData.Schema, modelPath);
    Console.WriteLine($"✓ Model saved to: {modelPath}");
    var fileInfo = new FileInfo(modelPath);
    Console.WriteLine($"  Model size: {fileInfo.Length / 1024:N0} KB");
}
catch (Exception ex)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"ERROR saving model: {ex.Message}");
    Console.ResetColor();
}

// ============================================================================
// 5. CREATE BASELINE MODELS
// ============================================================================
Console.WriteLine("\n=== Creating Baseline Models ===\n");

var randomBaseline = new BaselineRandom(minRating, maxRating, seed: 42);
Console.WriteLine($"✓ {randomBaseline.GetModelName()} initialized");
Console.WriteLine($"  Rating range: [{minRating:F1}, {maxRating:F1}]");

var popularBaseline = new BaselineMostPopular(trainList);
Console.WriteLine($"\n✓ {popularBaseline.GetModelName()} initialized");
Console.WriteLine($"  Known modules: {popularBaseline.KnownModulesCount}");
Console.WriteLine($"  Global average: {popularBaseline.GlobalAverage:F2}");

// ============================================================================
// 6. EVALUATE ALL MODELS
// ============================================================================
var metrics = Evaluation.EvaluateAllModels(
    mlContext,
    model,
    randomBaseline,
    popularBaseline,
    testList,
    minRating,
    maxRating
);

// ============================================================================
// 7. EXPORT RESULTS
// ============================================================================
Console.WriteLine("=== Exporting Results ===\n");

Directory.CreateDirectory("evaluation");

// Export metrics CSV
var metricsPath = Path.Combine("evaluation", "metrics.csv");
Evaluation.ExportMetricsToCSV(metrics, metricsPath);

// Generate text report
var reportPath = Path.Combine("evaluation", "report_text.txt");
Evaluation.GenerateTextReport(
    metrics,
    uniqueUsers,
    uniqueModules,
    totalRatings,
    trainList.Count,
    testList.Count,
    reportPath
);

// ============================================================================
// 8. GENERATE VISUALIZATIONS
// ============================================================================
var plotsFolder = Path.Combine("evaluation", "plots");
Visualization.GenerateAllPlots(metrics, plotsFolder);

// ============================================================================
// 9. CREATE HYBRID RECOMMENDER SYSTEM
// ============================================================================
Console.WriteLine("=== Creating Hybrid Recommender System ===\n");

var predictionEngine = mlContext.Model.CreatePredictionEngine<RatingInput, RatingPrediction>(model);
var contentBased = new ContentBasedRecommender(modules, users);
var hybridRecommender = new HybridRecommender(
    predictionEngine,
    contentBased,
    collaborativeWeight: 0.7f  // 70% collaborative, 30% content-based
);

Console.WriteLine("✓ Hybrid recommender created");
Console.WriteLine("  • Collaborative weight: 70%");
Console.WriteLine("  • Content-based weight: 30%");

// ============================================================================
// 10. DEMONSTRATION: Generate sample recommendations
// ============================================================================
Console.WriteLine("\n=== Sample Recommendations ===\n");

// Pick a random user from test set
var sampleUser = testList.First().UserId;
var userTestRatings = testList.Where(r => r.UserId == sampleUser).Take(3).ToList();

Console.WriteLine($"Sample user: {sampleUser}");
var userInfo = users.FirstOrDefault(u => u.UserId == sampleUser);
if (userInfo != null)
{
    Console.WriteLine($"  • Digital literacy: {userInfo.DigitalLiteracy}/5");
    Console.WriteLine($"  • Preferred topic: {userInfo.PreferredTopic}");
    Console.WriteLine($"  • Risk profile: {userInfo.RiskProfile}");
}

Console.WriteLine($"\nActual ratings from test set:");
foreach (var rating in userTestRatings)
{
    Console.WriteLine($"  • Module {rating.ModuleId}: {rating.Rating:F1}");
}

Console.WriteLine($"\nHybrid model predictions:");
foreach (var rating in userTestRatings)
{
    var hybridPred = hybridRecommender.Predict(rating.UserId, rating.ModuleId);
    var error = Math.Abs(rating.Rating - hybridPred);
    Console.WriteLine($"  • Module {rating.ModuleId}: {hybridPred:F2} (error: {error:F2})");
}

// Generate top-5 recommendations using hybrid approach
Console.WriteLine($"\nTop-5 recommended modules (HYBRID) for user {sampleUser}:");
var allModuleIds = allRatings.Select(r => r.ModuleId).Distinct().ToList();
var detailedRecs = hybridRecommender.GetDetailedRecommendations(sampleUser, allModuleIds, topN: 5);

for (int i = 0; i < detailedRecs.Count; i++)
{
    var rec = detailedRecs[i];
    var module = modules.FirstOrDefault(m => m.ModuleId == rec.moduleId);

    Console.WriteLine($"  {i + 1}. Module {rec.moduleId} - {module?.ScamType ?? "unknown"} " +
                     $"(difficulty: {module?.Difficulty ?? 0})");
    Console.WriteLine($"     Hybrid: {rec.hybridScore:F2} = Collab: {rec.collabScore:F2} + Content: {rec.contentScore:F2}");
}

// ============================================================================
// SUMMARY
// ============================================================================
Console.WriteLine("\n================================================================================");
Console.WriteLine("                              SUMMARY");
Console.WriteLine("================================================================================\n");

Console.WriteLine("Models trained and evaluated:");
foreach (var metric in metrics)
{
    Console.WriteLine($"  • {metric.ModelName}");
}

Console.WriteLine("\nGenerated outputs:");
Console.WriteLine($"  • Model:              {modelPath}");
Console.WriteLine($"  • Metrics CSV:        {metricsPath}");
Console.WriteLine($"  • Evaluation report:  {reportPath}");
Console.WriteLine($"  • Visualizations:     {plotsFolder}");

var bestModel = metrics.OrderBy(m => m.MAE).First();
Console.WriteLine($"\nBest performing model (by MAE): {bestModel.ModelName}");
Console.WriteLine($"  MAE:  {bestModel.MAE:F4}");
Console.WriteLine($"  RMSE: {bestModel.RMSE:F4}");
Console.WriteLine($"  R²:   {bestModel.RSquared:F4}");
if (bestModel.PrecisionAt5.HasValue)
{
    Console.WriteLine($"  P@5:  {bestModel.PrecisionAt5.Value:F4}");
}

Console.WriteLine("\n✓ Pipeline completed successfully!");
Console.WriteLine("\nNext steps:");
Console.WriteLine("  • Review the evaluation report: evaluation/report_text.txt");
Console.WriteLine("  • Examine visualizations in: evaluation/plots/");
Console.WriteLine("  • Analyze metrics in: evaluation/metrics.csv");
Console.WriteLine("  • Load the trained model from: model/model.zip");

Console.WriteLine("\n================================================================================");
