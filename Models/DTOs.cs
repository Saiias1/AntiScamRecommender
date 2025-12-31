namespace AntiScamAPI.Models;

// Request DTOs
public class RecommendationRequest
{
    public uint UserId { get; set; }
    public int Top { get; set; } = 5;
}

public class UserRegistrationRequest
{
    public string AgeGroup { get; set; } = string.Empty;
    public float DigitalLiteracy { get; set; }
    public string PreferredTopic { get; set; } = string.Empty;
    public string? RiskProfile { get; set; }
}

public class RatingSubmissionRequest
{
    public uint UserId { get; set; }
    public uint ModuleId { get; set; }
    public float Rating { get; set; }
}

// Response DTOs
public class RecommendationResponse
{
    public uint UserId { get; set; }
    public UserProfileDto? UserProfile { get; set; }
    public List<RecommendationDto> Recommendations { get; set; } = new();
}

public class UserProfileDto
{
    public float Literacy { get; set; }
    public string PreferredTopic { get; set; } = string.Empty;
    public string AgeGroup { get; set; } = string.Empty;
    public string RiskProfile { get; set; } = string.Empty;
}

public class RecommendationDto
{
    public uint ModuleId { get; set; }
    public string Title { get; set; } = string.Empty;
    public float HybridScore { get; set; }
    public float CollaborativeScore { get; set; }
    public float ContentScore { get; set; }
    public float Difficulty { get; set; }
    public string ScamType { get; set; } = string.Empty;
    public float TargetLiteracy { get; set; }
    public float DurationMin { get; set; }
}

public class UserRegistrationResponse
{
    public uint UserId { get; set; }
    public UserProfileDto Profile { get; set; } = new();
}

public class ModuleDetailResponse
{
    public uint ModuleId { get; set; }
    public string ScamType { get; set; } = string.Empty;
    public float Difficulty { get; set; }
    public float TargetLiteracy { get; set; }
    public float DurationMin { get; set; }
}

public class RatingSubmissionResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class HealthCheckResponse
{
    public string Status { get; set; } = string.Empty;
    public bool ModelLoaded { get; set; }
    public bool DataFilesLoaded { get; set; }
    public int TotalUsers { get; set; }
    public int TotalModules { get; set; }
    public int TotalRatings { get; set; }
}
