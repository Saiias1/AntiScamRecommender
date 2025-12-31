namespace AntiScamRecommender;

/// <summary>
/// Centralized configuration for the recommendation system
/// </summary>
public static class Configuration
{
    // Data paths
    public const string DataFolder = "data";
    public const string RatingsFile = "ratings.csv";
    public const string UsersFile = "users.csv";
    public const string ModulesFile = "modules.csv";

    // Model paths
    public const string ModelFolder = "model";
    public const string ModelFile = "model.zip";

    // Evaluation paths
    public const string EvaluationFolder = "evaluation";
    public const string MetricsFile = "metrics.csv";
    public const string ReportFile = "report_text.txt";
    public const string PlotsFolder = "plots";

    // Training parameters
    public static class Training
    {
        public const int RandomSeed = 42;
        public const double TestFraction = 0.2;
        public const int NumberOfIterations = 20;
        public const int ApproximationRank = 100;
        public const double LearningRate = 0.1;
    }

    // Evaluation parameters
    public static class Evaluation
    {
        public const int PrecisionK = 5;
    }

    // Visualization parameters
    public static class Visualization
    {
        public const int PlotWidth = 800;
        public const int PlotHeight = 600;
        public const int CombinedPlotWidth = 1000;
    }
}
