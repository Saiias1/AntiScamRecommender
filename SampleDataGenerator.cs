namespace AntiScamRecommender;

/// <summary>
/// Generates sample CSV files for testing the recommendation system
/// Use this if you don't have the actual datasets yet
/// </summary>
public static class SampleDataGenerator
{
    private static readonly Random Random = new Random(42);

    public static void GenerateSampleData(string dataFolder)
    {
        Console.WriteLine("\n=== Generating Sample Data ===\n");
        Console.WriteLine("NOTE: This generates synthetic sample data for testing purposes.");
        Console.WriteLine("Replace with your actual datasets when available.\n");

        Directory.CreateDirectory(dataFolder);

        GenerateUsers(Path.Combine(dataFolder, "users.csv"), 1000);
        GenerateModules(Path.Combine(dataFolder, "modules.csv"), 30);
        GenerateRatings(Path.Combine(dataFolder, "ratings.csv"), 1000, 30, 20000);

        Console.WriteLine("\n✓ Sample data generation complete!\n");
    }

    private static void GenerateUsers(string filePath, int numUsers)
    {
        var clusters = new[] { "tech_savvy", "average", "vulnerable", "senior", "youth" };
        var literacyLevels = new[] { "low", "medium", "high" };
        var ageGroups = new[] { "18-25", "26-35", "36-50", "51-65", "65+" };
        var riskProfiles = new[] { "low", "medium", "high" };
        var topics = new[] { "phishing", "malware", "social_engineering", "financial", "identity_theft" };

        var lines = new List<string> { "user_id,user_cluster,digital_literacy,age_group,risk_profile,preferred_topic" };

        for (int i = 1; i <= numUsers; i++)
        {
            lines.Add($"{i}," +
                     $"{clusters[Random.Next(clusters.Length)]}," +
                     $"{literacyLevels[Random.Next(literacyLevels.Length)]}," +
                     $"{ageGroups[Random.Next(ageGroups.Length)]}," +
                     $"{riskProfiles[Random.Next(riskProfiles.Length)]}," +
                     $"{topics[Random.Next(topics.Length)]}");
        }

        File.WriteAllLines(filePath, lines);
        Console.WriteLine($"  ✓ Generated {numUsers} users: {filePath}");
    }

    private static void GenerateModules(string filePath, int numModules)
    {
        var scamTypes = new[] { "phishing", "ransomware", "tech_support_scam", "romance_scam",
                                "investment_fraud", "identity_theft", "social_engineering",
                                "fake_charity", "lottery_scam", "employment_scam" };
        var difficulties = new[] { "beginner", "intermediate", "advanced" };
        var literacyLevels = new[] { "low", "medium", "high" };

        var lines = new List<string> { "module_id,scam_type,difficulty,target_literacy,duration_min" };

        for (int i = 1; i <= numModules; i++)
        {
            lines.Add($"{i}," +
                     $"{scamTypes[Random.Next(scamTypes.Length)]}," +
                     $"{difficulties[Random.Next(difficulties.Length)]}," +
                     $"{literacyLevels[Random.Next(literacyLevels.Length)]}," +
                     $"{Random.Next(10, 61)}");
        }

        File.WriteAllLines(filePath, lines);
        Console.WriteLine($"  ✓ Generated {numModules} modules: {filePath}");
    }

    private static void GenerateRatings(string filePath, int numUsers, int numModules, int numRatings)
    {
        var lines = new List<string> { "user_id,module_id,rating" };
        var usedPairs = new HashSet<(int, int)>();

        int attempts = 0;
        int maxAttempts = numRatings * 10;

        while (lines.Count - 1 < numRatings && attempts < maxAttempts)
        {
            int userId = Random.Next(1, numUsers + 1);
            int moduleId = Random.Next(1, numModules + 1);

            if (!usedPairs.Contains((userId, moduleId)))
            {
                usedPairs.Add((userId, moduleId));

                // Generate realistic ratings with a slight positive bias
                double rating = Random.NextDouble() * 2 + 2.5; // Range roughly 2.5 to 4.5
                rating += (Random.NextDouble() - 0.5); // Add some variance
                rating = Math.Max(1, Math.Min(5, rating)); // Clamp to [1, 5]

                lines.Add($"{userId},{moduleId},{rating:F1}");
            }

            attempts++;
        }

        if (lines.Count - 1 < numRatings)
        {
            Console.WriteLine($"  ⚠ Warning: Could only generate {lines.Count - 1} unique ratings (requested {numRatings})");
            Console.WriteLine($"    Consider reducing numRatings or increasing numUsers/numModules");
        }

        File.WriteAllLines(filePath, lines);
        Console.WriteLine($"  ✓ Generated {lines.Count - 1} ratings: {filePath}");
    }

    public static bool DataExists(string dataFolder)
    {
        var ratingsPath = Path.Combine(dataFolder, "ratings.csv");
        return File.Exists(ratingsPath);
    }

    public static void GenerateIfNeeded(string dataFolder)
    {
        if (!DataExists(dataFolder))
        {
            Console.WriteLine("No data found. Generating sample data...");
            GenerateSampleData(dataFolder);
        }
    }
}
