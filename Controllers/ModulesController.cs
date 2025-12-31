using Microsoft.AspNetCore.Mvc;
using AntiScamAPI.Models;
using AntiScamAPI.Services;

namespace AntiScamAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ModulesController : ControllerBase
{
    private readonly HybridRecommendationService _recommendationService;
    private readonly ILogger<ModulesController> _logger;

    public ModulesController(
        HybridRecommendationService recommendationService,
        ILogger<ModulesController> logger)
    {
        _recommendationService = recommendationService;
        _logger = logger;
    }

    [HttpGet("{moduleId}")]
    public ActionResult<ModuleDetailResponse> GetModule(uint moduleId)
    {
        try
        {
            var dataService = _recommendationService.GetDataService();
            var module = dataService.GetModule(moduleId);

            if (module == null)
            {
                return NotFound(new { message = $"Module {moduleId} not found" });
            }

            var response = new ModuleDetailResponse
            {
                ModuleId = module.ModuleId,
                ScamType = module.ScamType,
                Difficulty = module.Difficulty,
                TargetLiteracy = module.TargetLiteracy,
                DurationMin = module.DurationMin
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving module {ModuleId}", moduleId);
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }

    [HttpGet]
    public ActionResult<List<ModuleDetailResponse>> GetAllModules()
    {
        try
        {
            var dataService = _recommendationService.GetDataService();
            var modules = dataService.LoadModules();

            var response = modules.Select(m => new ModuleDetailResponse
            {
                ModuleId = m.ModuleId,
                ScamType = m.ScamType,
                Difficulty = m.Difficulty,
                TargetLiteracy = m.TargetLiteracy,
                DurationMin = m.DurationMin
            }).ToList();

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving modules");
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }
}
