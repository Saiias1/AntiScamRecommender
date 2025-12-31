namespace AntiScamRecommender;

/// <summary>
/// Prediction output for Matrix Factorization model
/// </summary>
public class RatingPrediction
{
    public float Score { get; set; }

    public override string ToString()
    {
        return $"Predicted Rating: {Score:F2}";
    }
}
