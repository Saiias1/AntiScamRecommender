# Quick Start Guide

## Snelle test zonder eigen data

Als je het project direct wil testen zonder eerst CSV bestanden te maken:

### Optie 1: Automatische sample data generatie

```bash
cd AntiScamRecommender
dotnet run
```

Het programma zal vragen of je sample data wilt genereren. Type `y` en druk op Enter.

### Optie 2: Met Visual Studio

1. Open `AntiScamRecommender.sln` in Visual Studio 2022
2. Druk op F5 of klik op "Start"
3. Type `y` wanneer gevraagd om sample data te genereren

## Met eigen data

### Stap 1: Plaats je CSV bestanden

Maak een `data/` folder in het project en plaats daar:

```
AntiScamRecommender/data/
├── users.csv      (1000 rijen)
├── modules.csv    (30 rijen)
└── ratings.csv    (20000 rijen)
```

### Stap 2: Controleer CSV structuur

**users.csv**
```csv
user_id,user_cluster,digital_literacy,age_group,risk_profile,preferred_topic
1,tech_savvy,high,26-35,low,phishing
2,vulnerable,low,65+,high,social_engineering
...
```

**modules.csv**
```csv
module_id,scam_type,difficulty,target_literacy,duration_min
1,phishing,beginner,low,15
2,ransomware,advanced,high,45
...
```

**ratings.csv**
```csv
user_id,module_id,rating
1,5,4.5
1,12,3.0
2,3,5.0
...
```

### Stap 3: Run het project

```bash
cd AntiScamRecommender
dotnet run
```

## Output

Na het runnen vind je:

```
AntiScamRecommender/
├── model/
│   └── model.zip                        # Trained model
├── evaluation/
│   ├── metrics.csv                      # Metrics vergelijking
│   ├── report_text.txt                  # Uitgebreid rapport
│   └── plots/
│       ├── mae_comparison.png
│       ├── rmse_comparison.png
│       ├── r_squared_comparison.png
│       └── combined_metrics.png
```

## Belangrijkste bestanden

| Bestand | Beschrijving |
|---------|-------------|
| `evaluation/report_text.txt` | Lees dit eerst! Volledige evaluatie met analyse |
| `evaluation/metrics.csv` | Numerieke vergelijking van alle modellen |
| `evaluation/plots/` | Visuele vergelijking van model performance |
| `model/model.zip` | Opgeslagen model (kan geladen worden voor predictions) |

## Voorbeeld console output

```
================================================================================
              ANTI-SCAM RECOMMENDER SYSTEM - ML.NET
================================================================================

=== Loading Data ===
✓ Loaded ratings from: data/ratings.csv

Dataset Statistics:
  • Total ratings:     20,000
  • Unique users:      1,000
  • Unique modules:    30
  • Rating range:      [1.0, 5.0]
  • Average rating:    3.45
  • Sparsity:          33.33%

=== Training Matrix Factorization Model ===
Training parameters:
  • Iterations:        20
  • Latent factors:    100
  • Learning rate:     0.1

✓ Matrix Factorization model trained successfully!

=== Evaluating Models ===
✓ All models evaluated successfully!

=== Exporting Results ===
✓ Metrics exported to: evaluation/metrics.csv
✓ Evaluation report generated: evaluation/report_text.txt

=== Generating Visualizations ===
  ✓ MAE comparison saved
  ✓ RMSE comparison saved
  ✓ R² comparison saved
  ✓ Combined metrics chart saved

✓ Pipeline completed successfully!
```

## Troubleshooting

### "File not found" error

Zorg dat je CSV bestanden in de `data/` folder staan met exacte namen:
- `users.csv`
- `modules.csv`
- `ratings.csv`

### CSV parsing errors

Controleer:
- Hebben alle CSV's headers (eerste regel)?
- Zijn kolommen comma-separated?
- Zijn alle user_id en module_id numeriek?
- Zijn ratings numerieke waarden?

### Build errors

```bash
dotnet clean
dotnet restore
dotnet build
```

### Visualisatie fouten

ScottPlot heeft native dependencies. Als je errors krijgt:
- Installeer de latest .NET 9 SDK
- Op Linux: installeer `libgdiplus`
- Op macOS: installeer via `brew install mono-libgdiplus`

## Model laden voor predictions

Na training kun je het model laden en gebruiken:

```csharp
var mlContext = new MLContext();
ITransformer model;

using (var stream = File.OpenRead("model/model.zip"))
{
    model = mlContext.Model.Load(stream, out var schema);
}

var predictionEngine = mlContext.Model
    .CreatePredictionEngine<RatingInput, RatingPrediction>(model);

var prediction = predictionEngine.Predict(new RatingInput
{
    UserId = 123,
    ModuleId = 5,
    Rating = 0 // Wordt genegeerd bij prediction
});

Console.WriteLine($"Predicted rating: {prediction.Score:F2}");
```

## Volgende stappen

1. **Bekijk het rapport**: Open `evaluation/report_text.txt`
2. **Analyseer visualisaties**: Check `evaluation/plots/*.png`
3. **Vergelijk metrics**: Open `evaluation/metrics.csv` in Excel
4. **Experimenteer**: Pas hyperparameters aan in `Configuration.cs`
5. **Gebruik het model**: Laad `model.zip` voor predictions

## Support

Voor vragen of problemen, check de volledige README.md voor gedetailleerde documentatie.
