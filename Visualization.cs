using ScottPlot;

namespace AntiScamRecommender;

/// <summary>
/// Visualization module for generating evaluation charts using ScottPlot
/// </summary>
public static class Visualization
{
    /// <summary>
    /// Generate all evaluation plots
    /// </summary>
    public static void GenerateAllPlots(List<Evaluation.ModelMetrics> metrics, string outputFolder)
    {
        Console.WriteLine("\n=== Generating Visualizations ===\n");

        Directory.CreateDirectory(outputFolder);

        GenerateMAEComparison(metrics, Path.Combine(outputFolder, "mae_comparison.png"));
        GenerateRMSEComparison(metrics, Path.Combine(outputFolder, "rmse_comparison.png"));
        GenerateRSquaredComparison(metrics, Path.Combine(outputFolder, "r_squared_comparison.png"));
        GenerateCombinedMetrics(metrics, Path.Combine(outputFolder, "combined_metrics.png"));

        Console.WriteLine("\n✓ All visualizations generated successfully!\n");
    }

    /// <summary>
    /// Generate MAE comparison bar chart
    /// </summary>
    private static void GenerateMAEComparison(List<Evaluation.ModelMetrics> metrics, string outputPath)
    {
        var plot = new Plot();

        var modelNames = metrics.Select(m => m.ModelName).ToArray();
        var maeValues = metrics.Select(m => m.MAE).ToArray();

        var barPlot = plot.Add.Bars(maeValues);

        // Set colors - ScottPlot 5.x uses Color property
        foreach (var bar in barPlot.Bars)
        {
            bar.FillColor = Colors.DodgerBlue;
            bar.LineColor = Colors.Navy;
            bar.LineWidth = 1;
        }

        // Customize axes
        plot.Axes.Bottom.TickGenerator = new ScottPlot.TickGenerators.NumericManual(
            Enumerable.Range(0, modelNames.Length).Select(i => (double)i).ToArray(),
            modelNames
        );
        plot.Axes.Bottom.MajorTickStyle.Length = 0;

        plot.Axes.Left.Label.Text = "MAE (Mean Absolute Error)";
        plot.Axes.Bottom.Label.Text = "Model";
        plot.Title("MAE Comparison: Lower is Better");

        // Style
        plot.Axes.Margins(bottom: 0);
        plot.FigureBackground.Color = Colors.WhiteSmoke;
        plot.DataBackground.Color = Colors.White;

        // Add grid
        plot.Grid.MajorLineColor = Colors.LightGray.WithAlpha(0.5);

        plot.SavePng(outputPath, 800, 600);
        Console.WriteLine($"  ✓ MAE comparison saved: {outputPath}");
    }

    /// <summary>
    /// Generate RMSE comparison bar chart
    /// </summary>
    private static void GenerateRMSEComparison(List<Evaluation.ModelMetrics> metrics, string outputPath)
    {
        var plot = new Plot();

        var modelNames = metrics.Select(m => m.ModelName).ToArray();
        var rmseValues = metrics.Select(m => m.RMSE).ToArray();

        var barPlot = plot.Add.Bars(rmseValues);

        // Set colors
        foreach (var bar in barPlot.Bars)
        {
            bar.FillColor = Colors.Coral;
            bar.LineColor = Colors.DarkRed;
            bar.LineWidth = 1;
        }

        // Customize axes
        plot.Axes.Bottom.TickGenerator = new ScottPlot.TickGenerators.NumericManual(
            Enumerable.Range(0, modelNames.Length).Select(i => (double)i).ToArray(),
            modelNames
        );
        plot.Axes.Bottom.MajorTickStyle.Length = 0;

        plot.Axes.Left.Label.Text = "RMSE (Root Mean Squared Error)";
        plot.Axes.Bottom.Label.Text = "Model";
        plot.Title("RMSE Comparison: Lower is Better");

        // Style
        plot.Axes.Margins(bottom: 0);
        plot.FigureBackground.Color = Colors.WhiteSmoke;
        plot.DataBackground.Color = Colors.White;

        plot.Grid.MajorLineColor = Colors.LightGray.WithAlpha(0.5);

        plot.SavePng(outputPath, 800, 600);
        Console.WriteLine($"  ✓ RMSE comparison saved: {outputPath}");
    }

    /// <summary>
    /// Generate R² comparison bar chart
    /// </summary>
    private static void GenerateRSquaredComparison(List<Evaluation.ModelMetrics> metrics, string outputPath)
    {
        var plot = new Plot();

        var modelNames = metrics.Select(m => m.ModelName).ToArray();
        var r2Values = metrics.Select(m => m.RSquared).ToArray();

        var barPlot = plot.Add.Bars(r2Values);

        // Set colors
        foreach (var bar in barPlot.Bars)
        {
            bar.FillColor = Colors.MediumSeaGreen;
            bar.LineColor = Colors.DarkGreen;
            bar.LineWidth = 1;
        }

        // Customize axes
        plot.Axes.Bottom.TickGenerator = new ScottPlot.TickGenerators.NumericManual(
            Enumerable.Range(0, modelNames.Length).Select(i => (double)i).ToArray(),
            modelNames
        );
        plot.Axes.Bottom.MajorTickStyle.Length = 0;

        plot.Axes.Left.Label.Text = "R² (Coefficient of Determination)";
        plot.Axes.Bottom.Label.Text = "Model";
        plot.Title("R² Comparison: Higher is Better (1 = Perfect Fit)");

        // Style
        plot.Axes.Margins(bottom: 0);
        plot.FigureBackground.Color = Colors.WhiteSmoke;
        plot.DataBackground.Color = Colors.White;

        plot.Grid.MajorLineColor = Colors.LightGray.WithAlpha(0.5);

        // Set Y-axis range for R² (typically -infinity to 1, but limit for readability)
        var minR2 = r2Values.Min();
        plot.Axes.SetLimitsY(Math.Min(minR2 - 0.1, -0.5), 1.1);

        plot.SavePng(outputPath, 800, 600);
        Console.WriteLine($"  ✓ R² comparison saved: {outputPath}");
    }

    /// <summary>
    /// Generate combined metrics visualization with normalized values
    /// </summary>
    private static void GenerateCombinedMetrics(List<Evaluation.ModelMetrics> metrics, string outputPath)
    {
        var plot = new Plot();

        var modelNames = metrics.Select(m => m.ModelName).ToArray();

        // Normalize MAE and RMSE to [0, 1] for comparison
        var maeValues = metrics.Select(m => m.MAE).ToArray();
        var rmseValues = metrics.Select(m => m.RMSE).ToArray();
        var r2Values = metrics.Select(m => m.RSquared).ToArray();

        var maxMAE = maeValues.Max();
        var maxRMSE = rmseValues.Max();

        // Normalize (invert for MAE/RMSE so higher is better for visualization consistency)
        var maeNormalized = maeValues.Select(v => 1 - (v / maxMAE)).ToArray();
        var rmseNormalized = rmseValues.Select(v => 1 - (v / maxRMSE)).ToArray();
        // R² already on [0,1] scale and higher is better, but handle negative values
        var r2Normalized = r2Values.Select(v => Math.Max(0, v)).ToArray();

        // Create grouped bar chart
        double[] positions = Enumerable.Range(0, modelNames.Length).Select(i => (double)i).ToArray();
        double barWidth = 0.25;

        // Add bars for each metric
        var maeBars = plot.Add.Bars(positions.Select(p => p - barWidth).ToArray(), maeNormalized);
        foreach (var bar in maeBars.Bars)
        {
            bar.FillColor = Colors.DodgerBlue;
            bar.LineColor = Colors.Navy;
            bar.LineWidth = 1;
        }
        maeBars.LegendText = "MAE (normalized)";

        var rmseBars = plot.Add.Bars(positions.ToArray(), rmseNormalized);
        foreach (var bar in rmseBars.Bars)
        {
            bar.FillColor = Colors.Coral;
            bar.LineColor = Colors.DarkRed;
            bar.LineWidth = 1;
        }
        rmseBars.LegendText = "RMSE (normalized)";

        var r2Bars = plot.Add.Bars(positions.Select(p => p + barWidth).ToArray(), r2Normalized);
        foreach (var bar in r2Bars.Bars)
        {
            bar.FillColor = Colors.MediumSeaGreen;
            bar.LineColor = Colors.DarkGreen;
            bar.LineWidth = 1;
        }
        r2Bars.LegendText = "R² (actual)";

        // Customize axes
        plot.Axes.Bottom.TickGenerator = new ScottPlot.TickGenerators.NumericManual(
            positions,
            modelNames
        );
        plot.Axes.Bottom.MajorTickStyle.Length = 0;

        plot.Axes.Left.Label.Text = "Normalized Score (Higher is Better)";
        plot.Axes.Bottom.Label.Text = "Model";
        plot.Title("Combined Metrics Comparison");

        // Add legend
        plot.ShowLegend(Alignment.UpperRight);

        // Style
        plot.Axes.Margins(bottom: 0);
        plot.FigureBackground.Color = Colors.WhiteSmoke;
        plot.DataBackground.Color = Colors.White;

        plot.Grid.MajorLineColor = Colors.LightGray.WithAlpha(0.5);

        // Set Y-axis
        plot.Axes.SetLimitsY(0, 1.1);

        plot.SavePng(outputPath, 1000, 600);
        Console.WriteLine($"  ✓ Combined metrics chart saved: {outputPath}");
    }
}
