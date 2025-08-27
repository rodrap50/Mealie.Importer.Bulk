using API.Infrastructure.Models;
using API.Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MealieImporterController(IBulkImportService bulkImportService, ILogger<MealieImporterController> logger) : ControllerBase
    {
        private readonly IBulkImportService _bulkImportService = bulkImportService;
    private readonly ILogger<MealieImporterController> _logger = logger;

    [HttpPost("Bulk")]
    public async Task<IActionResult> BulkImportRecipes([FromBody] List<Recipe> recipes)
    {
        try
        {
            _logger.LogInformation("Starting bulk import of {recipesCount} recipes", recipes.Count );
            
            var result = await _bulkImportService.ImportRecipesAsync(recipes);
            
            _logger.LogInformation("Bulk import completed. Success: {resultSuccessCount}, Failed: {resultFailureCount}", result.SuccessCount, result.FailureCount);
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during bulk import");
            return StatusCode(500, new { message = "An error occurred during bulk import", error = ex.Message });
        }
    }
    }
}
