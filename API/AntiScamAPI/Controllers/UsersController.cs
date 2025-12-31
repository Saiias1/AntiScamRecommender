using Microsoft.AspNetCore.Mvc;
using AntiScamAPI.Models;
using AntiScamAPI.Services;

namespace AntiScamAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly HybridRecommendationService _recommendationService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(
        HybridRecommendationService recommendationService,
        ILogger<UsersController> logger)
    {
        _recommendationService = recommendationService;
        _logger = logger;
    }

    [HttpPost("register")]
    public ActionResult<UserRegistrationResponse> RegisterUser([FromBody] UserRegistrationRequest request)
    {
        try
        {
            _logger.LogInformation("Registering new user: {AgeGroup}, {Literacy}, {Topic}",
                request.AgeGroup, request.DigitalLiteracy, request.PreferredTopic);

            var dataService = _recommendationService.GetDataService();

            // Generate new user ID
            var userId = dataService.GetNextUserId();

            // Determine risk profile based on literacy if not provided
            var riskProfile = request.RiskProfile ?? (request.DigitalLiteracy <= 2 ? "high" :
                                                      request.DigitalLiteracy <= 3 ? "medium" : "low");

            // Assign user cluster (simplified - could be more sophisticated)
            var userCluster = (int)(request.DigitalLiteracy % 5) + 1;

            var newUser = new UserInput
            {
                UserId = userId,
                UserCluster = userCluster,
                DigitalLiteracy = request.DigitalLiteracy,
                AgeGroup = request.AgeGroup,
                RiskProfile = riskProfile,
                PreferredTopic = request.PreferredTopic
            };

            // Save to CSV
            dataService.AppendUser(newUser);

            // Add to in-memory cache
            _recommendationService.AddUser(newUser);

            var response = new UserRegistrationResponse
            {
                UserId = userId,
                Profile = new UserProfileDto
                {
                    Literacy = newUser.DigitalLiteracy,
                    PreferredTopic = newUser.PreferredTopic,
                    AgeGroup = newUser.AgeGroup,
                    RiskProfile = newUser.RiskProfile
                }
            };

            _logger.LogInformation("User registered successfully with ID {UserId}", userId);
            return CreatedAtAction(nameof(RegisterUser), new { id = userId }, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering user");
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }

    [HttpGet("{userId}")]
    public ActionResult<UserProfileDto> GetUser(uint userId)
    {
        try
        {
            // Try cache first (includes newly registered users)
            var user = _recommendationService.GetCachedUser(userId);

            if (user == null)
            {
                return NotFound(new { message = $"User {userId} not found" });
            }

            var profile = new UserProfileDto
            {
                Literacy = user.DigitalLiteracy,
                PreferredTopic = user.PreferredTopic,
                AgeGroup = user.AgeGroup,
                RiskProfile = user.RiskProfile
            };

            return Ok(profile);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user {UserId}", userId);
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }
}
