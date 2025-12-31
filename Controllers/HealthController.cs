using Microsoft.AspNetCore.Mvc;
using AntiScamAPI.Models;
using AntiScamAPI.Services;

namespace AntiScamAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly HybridRecommendationService _recommendationService;
    private readonly ILogger<HealthController> _logger;

    public HealthController(
        HybridRecommendationService recommendationService,
        ILogger<HealthController> logger)
    {
        _recommendationService = recommendationService;
        _logger = logger;
    }

    [HttpGet]
    public ActionResult<HealthCheckResponse> GetHealth()
    {
        try
        {
            var dataService = _recommendationService.GetDataService();

            var users = dataService.LoadUsers();
            var modules = dataService.LoadModules();
            var ratings = dataService.LoadRatings();

            var response = new HealthCheckResponse
            {
                Status = "Healthy",
                ModelLoaded = true,
                DataFilesLoaded = true,
                TotalUsers = users.Count,
                TotalModules = modules.Count,
                TotalRatings = ratings.Count
            };

            _logger.LogInformation("Health check successful: {Users} users, {Modules} modules, {Ratings} ratings",
                users.Count, modules.Count, ratings.Count);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Health check failed");
            return StatusCode(500, new HealthCheckResponse
            {
                Status = "Unhealthy",
                ModelLoaded = false,
                DataFilesLoaded = false
            });
        }
    }
}
