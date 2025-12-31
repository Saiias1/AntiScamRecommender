using Microsoft.ML.Data;

namespace AntiScamRecommender;

public class ModuleInput
{
    [LoadColumn(0)]
    public uint ModuleId { get; set; }

    [LoadColumn(1)]
    public string ScamType { get; set; } = string.Empty;

    [LoadColumn(2)]
    public float Difficulty { get; set; }

    [LoadColumn(3)]
    public float TargetLiteracy { get; set; }

    [LoadColumn(4)]
    public float DurationMin { get; set; }
}
