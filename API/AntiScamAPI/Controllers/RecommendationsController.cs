using Microsoft.AspNetCore.Mvc;
using AntiScamAPI.Models;
using AntiScamAPI.Services;

namespace AntiScamAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RecommendationsController : ControllerBase
{
    private readonly HybridRecommendationService _recommendationService;
    private readonly ILogger<RecommendationsController> _logger;

    public RecommendationsController(
        HybridRecommendationService recommendationService,
        ILogger<RecommendationsController> logger)
    {
        _recommendationService = recommendationService;
        _logger = logger;
    }

    [HttpPost]
    public ActionResult<RecommendationResponse> GetRecommendations([FromBody] RecommendationRequest request)
    {
        try
        {
            _logger.LogInformation("Getting recommendations for user {UserId}, top {Top}", request.UserId, request.Top);

            // Check cache first (includes new users)
            var user = _recommendationService.GetCachedUser(request.UserId);

            if (user == null)
            {
                return NotFound(new { message = $"User {request.UserId} not found" });
            }

            // Check if user has any ratings (to decide hybrid vs content-based only)
            var dataService = _recommendationService.GetDataService();
            var allRatings = dataService.LoadRatings();
            var userHasRatings = allRatings.Any(r => r.UserId == request.UserId);

            // Get recommendations (content-based only for new users without ratings)
            var recommendations = _recommendationService.GetRecommendations(
                request.UserId,
                request.Top,
                contentBasedOnly: !userHasRatings
            );

            var modules = _recommendationService.GetModules();

            if (!userHasRatings)
            {
                _logger.LogInformation("User {UserId} has no ratings - using content-based recommendations only", request.UserId);
            }

            var response = new RecommendationResponse
            {
                UserId = request.UserId,
                UserProfile = new UserProfileDto
                {
                    Literacy = user.DigitalLiteracy,
                    PreferredTopic = user.PreferredTopic,
                    AgeGroup = user.AgeGroup,
                    RiskProfile = user.RiskProfile
                },
                Recommendations = recommendations.Select(rec =>
                {
                    var module = modules.FirstOrDefault(m => m.ModuleId == rec.moduleId);
                    return new RecommendationDto
                    {
                        ModuleId = rec.moduleId,
                        Title = module != null ? $"{module.ScamType} Training Module" : "Unknown Module",
                        HybridScore = rec.hybridScore,
                        CollaborativeScore = rec.collabScore,
                        ContentScore = rec.contentScore,
                        Difficulty = module?.Difficulty ?? 0,
                        ScamType = module?.ScamType ?? "unknown",
                        TargetLiteracy = module?.TargetLiteracy ?? 0,
                        DurationMin = module?.DurationMin ?? 0
                    };
                }).ToList()
            };

            _logger.LogInformation("Generated {Count} recommendations for user {UserId}", response.Recommendations.Count, request.UserId);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating recommendations for user {UserId}", request.UserId);
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }
}
