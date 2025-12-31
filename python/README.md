# Python Implementation - Anti-Scam Recommender

Dit is de Python implementatie van het Anti-Scam Recommender systeem, parallel aan de C# ML.NET versie.

## Overzicht

Dit notebook implementeert een **hybride recommender systeem** dat combineert:
- **Collaborative Filtering**: SVD (Singular Value Decomposition) matrix factorization
- **Content-Based Filtering**: User/module feature matching (scam type, difficulty, literacy level, duration)
- **Hybrid Approach**: Weighted combination (70% collaborative + 30% content-based)

Het gebruikt alleen **scipy en sklearn** - geen externe libraries die compilatie nodig hebben!

## Installatie

### Vereisten
- Python 3.8+
- Jupyter Notebook of JupyterLab

### Dependencies installeren

```bash
pip install pandas numpy matplotlib seaborn scikit-learn scipy
```

Alle packages zijn standaard Python scientific libraries - geen C++ compiler nodig!

## Gebruik

### 1. Start Jupyter

```bash
cd python
jupyter notebook
```

### 2. Open ModelTraining.ipynb

Open `ModelTraining.ipynb` in Jupyter en voer alle cells uit (Cell > Run All).

### 3. Volgorde

Het notebook doorloopt:
1. **Imports** - Laadt pandas, numpy, sklearn, scipy
2. **Data laden** - Laadt ratings.csv, modules.csv, users.csv
3. **Train/test split** - 80/20 split (random_state=42)
4. **Collaborative Filtering (SVD)** - Matrix factorization met scipy
5. **Evaluatie Collaborative** - MAE & RMSE berekenen
6. **Content-Based Filtering** - Feature matching (topic, literacy, difficulty, duration)
7. **Hybrid Recommender** - 70% collaborative + 30% content-based
8. **Demo** - Top-5 aanbevelingen voor sample user
9. **Visualisaties** - Vergelijking van alle modellen
10. **Resultaten opslaan** - Export naar CSV

## Output

Na uitvoering worden gegenereerd:

```
python/
├── ModelTraining.ipynb       # Het notebook
├── results.csv                # Model metrics
├── best_hyperparameters.csv   # Optimale parameters
└── plots/
    ├── mae_comparison.png     # MAE: Python vs C#
    ├── rmse_comparison.png    # RMSE: Python vs C#
    ├── hyperparameter_impact.png
    └── cross_validation.png
```

## Parameters

### SVD Model (standaard)

```python
svd_params = {
    'n_factors': 100,      # Aantal latente factoren
    'n_epochs': 20,        # Training iteraties
    'lr_all': 0.1,         # Learning rate
    'random_state': 42     # Voor reproduceerbaarheid
}
```

Deze parameters komen overeen met de C# implementatie:
- `n_factors` = `ApproximationRank`
- `n_epochs` = `NumberOfIterations`
- `lr_all` = `LearningRate`

### Hyperparameter Grid

```python
param_grid = {
    'n_factors': [50, 100, 150],
    'n_epochs': [10, 20, 40],
    'lr_all': [0.01, 0.05, 0.1]
}
```

## Metrics

Het notebook berekent:

- **MAE** (Mean Absolute Error) - Gemiddelde absolute voorspellingsfout
- **RMSE** (Root Mean Squared Error) - Wortel van gemiddelde kwadratische fout
- **Precision@5** - Proportie relevante items in top-5 aanbevelingen
- **Cross-validation scores** - Gemiddelde + standaarddeviatie over 5 folds

## Vergelijking met C#

Het notebook vergelijkt automatisch met C# resultaten als `../evaluation/metrics.csv` bestaat.

Verwachte resultaten (afhankelijk van data):
- Python SVD en C# Matrix Factorization zouden vergelijkbare prestaties moeten hebben
- Verschillen < 5% zijn normaal door algoritme implementatie verschillen
- Beide zouden significant beter moeten zijn dan random baseline

## Troubleshooting

### Import errors

```bash
pip install --upgrade scikit-surprise
```

### Notebook niet gevonden

Zorg dat je in de `python/` folder zit:

```bash
cd "c:\Users\Admin\OneDrive - Thomas More\AI project\Succes\Model\AntiScamRecommender\python"
```

### C# metrics niet gevonden

Voer eerst het C# project uit:

```bash
cd ..
dotnet run
```

### Slow grid search

Grid search kan 2-5 minuten duren. Voor snellere tests, reduceer de parameter grid:

```python
param_grid = {
    'n_factors': [100],
    'n_epochs': [20],
    'lr_all': [0.1]
}
```

## Dataset

Het notebook verwacht deze structuur:

```
../data/ratings.csv
```

Met kolommen:
- `user_id` (integer)
- `module_id` (integer)
- `rating` (float, 1.0-5.0)

## Aanpassen

### Andere algoritmes testen

Surprise ondersteunt meerdere algoritmes:

```python
from surprise import KNNBasic, BaselineOnly, NMF, SlopeOne

# K-Nearest Neighbors
knn_model = KNNBasic()

# Non-negative Matrix Factorization
nmf_model = NMF()
```

### Rating threshold wijzigen

Voor Precision@K berekening:

```python
precision_at_k(predictions, k=5, threshold=4.0)  # Strengere threshold
```

### Meer cross-validation folds

```python
cv_results = cross_validate(svd_model, data, cv=10)  # 10-fold
```

## Documentatie

- [Surprise Library](https://surpriselib.com/)
- [Surprise API Docs](https://surprise.readthedocs.io/)
- [Matrix Factorization](https://en.wikipedia.org/wiki/Matrix_factorization_(recommender_systems))

## Contact

Voor vragen over de Python implementatie, check de comments in het notebook of raadpleeg de Surprise documentatie.
