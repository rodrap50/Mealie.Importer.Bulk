namespace API.Infrastructure.Services;
using Models;
using Models.Response;
public interface IMealieApiClient
{
    Task<MealieItemsResponse<MealieTag>> GetTagsAsync();
    Task<MealieTag> CreateTagAsync(string name);
    Task<MealieItemsResponse<MealieCategory>> GetCategoriesAsync();
    Task<MealieCategory> CreateCategoryAsync(string name);
    Task<MealieItemsResponse<MealieTool>> GetToolsAsync();
    Task<MealieTool> CreateToolAsync(string name);
    Task<string> CreateRecipeAsync(Recipe recipe);
    Task<List<ParsedIngredient>> ParseIngredientsAsync(List<string> ingredients);
    Task UpdateRecipeIngredientsAsync(string recipeName, List<ParsedIngredient> ingredients);
    Task UpdateRecipeCategoriesAsync(string recipeName, List<MealieCategory> categories);
    Task UpdateRecipeToolsAsync(string recipeName, List<MealieTool> tools);
    Task<string> CreateFoodAsync(MealieFood food);
}
