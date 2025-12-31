using Microsoft.AspNetCore.Mvc;
using AntiScamAPI.Models;
using AntiScamAPI.Services;

namespace AntiScamAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RatingsController : ControllerBase
{
    private readonly HybridRecommendationService _recommendationService;
    private readonly ILogger<RatingsController> _logger;

    public RatingsController(
        HybridRecommendationService recommendationService,
        ILogger<RatingsController> logger)
    {
        _recommendationService = recommendationService;
        _logger = logger;
    }

    [HttpPost]
    public ActionResult<RatingSubmissionResponse> SubmitRating([FromBody] RatingSubmissionRequest request)
    {
        try
        {
            _logger.LogInformation("Submitting rating: User {UserId}, Module {ModuleId}, Rating {Rating}",
                request.UserId, request.ModuleId, request.Rating);

            // Validate rating range
            if (request.Rating < 1.0f || request.Rating > 5.0f)
            {
                return BadRequest(new { message = "Rating must be between 1.0 and 5.0" });
            }

            var dataService = _recommendationService.GetDataService();

            // Verify user exists
            var user = dataService.GetUser(request.UserId);
            if (user == null)
            {
                return NotFound(new { message = $"User {request.UserId} not found" });
            }

            // Verify module exists
            var module = dataService.GetModule(request.ModuleId);
            if (module == null)
            {
                return NotFound(new { message = $"Module {request.ModuleId} not found" });
            }

            // Save rating
            var rating = new RatingInput
            {
                UserId = request.UserId,
                ModuleId = request.ModuleId,
                Rating = request.Rating
            };

            dataService.AppendRating(rating);

            var response = new RatingSubmissionResponse
            {
                Success = true,
                Message = $"Rating submitted successfully for user {request.UserId} on module {request.ModuleId}"
            };

            _logger.LogInformation("Rating submitted successfully");
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting rating");
            return StatusCode(500, new
            {
                message = "Internal server error",
                error = ex.Message
            });
        }
    }
}
