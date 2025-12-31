using Microsoft.ML.Data;

namespace AntiScamRecommender;

public class UserInput
{
    [LoadColumn(0)]
    public uint UserId { get; set; }

    [LoadColumn(1)]
    public int UserCluster { get; set; }

    [LoadColumn(2)]
    public float DigitalLiteracy { get; set; }

    [LoadColumn(3)]
    public string AgeGroup { get; set; } = string.Empty;

    [LoadColumn(4)]
    public string RiskProfile { get; set; } = string.Empty;

    [LoadColumn(5)]
    public string PreferredTopic { get; set; } = string.Empty;
}
