namespace API.Infrastructure.Services;
using Infrastructure.Models;

public interface IBulkImportService
{
    Task<BulkImportResult> ImportRecipesAsync(List<Recipe> recipes);
}
