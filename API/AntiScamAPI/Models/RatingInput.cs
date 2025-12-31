using Microsoft.ML.Data;

namespace AntiScamAPI.Models;

/// <summary>
/// Represents a user-item interaction for the recommendation system
/// </summary>
public class RatingInput
{
    [LoadColumn(0)]
    public uint UserId { get; set; }

    [LoadColumn(1)]
    public uint ModuleId { get; set; }

    [LoadColumn(2)]
    public float Rating { get; set; }

    public override string ToString()
    {
        return $"User {UserId} rated Module {ModuleId} with {Rating}";
    }
}
