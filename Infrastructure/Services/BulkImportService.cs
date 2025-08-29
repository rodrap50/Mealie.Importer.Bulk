using API.Infrastructure.Models;

namespace API.Infrastructure.Services
{
    public class BulkImportService(IMealieApiClient mealieClient, ILogger<BulkImportService> logger) : IBulkImportService
    {
        private readonly IMealieApiClient _mealieClient = mealieClient;
        private readonly ILogger<BulkImportService> _logger = logger;

        public async Task<BulkImportResult> ImportRecipesAsync(List<Recipe> recipes)
        {
            var result = new BulkImportResult();
        
            try
            {
                // Prepare all necessary data
                var tagMap = await PrepareTagsAsync(recipes);
                var categoryMap = await PrepareCategoriesAsync(recipes);
                var toolMap = await PrepareToolsAsync(recipes);

                // Import recipes
                foreach (var recipe in recipes)
                {
                    try
                    {
                        await ImportSingleRecipeAsync(recipe, categoryMap, toolMap, result);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to import recipe: {recipeName}", recipe.Name);
                        result.FailureCount++;
                        result.FailedRecipes.Add(recipe.Name);
                        result.ErrorMessages.Add($"{recipe.Name}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during bulk import preparation");
                result.ErrorMessages.Add($"Preparation error: {ex.Message}");
            }

            return result;
        }

        private async Task<Dictionary<string, MealieTag>> PrepareTagsAsync(List<Recipe> recipes)
        {
            _logger.LogInformation("Preparing tags...");
        
            var existingTags = await _mealieClient.GetTagsAsync();
            var tagMap = existingTags.Items.ToDictionary(t => t.Name, t => t);

            var allKeywords = recipes
                .SelectMany(r => r.Keywords)
                .Where(k => !string.IsNullOrEmpty(k))
                .Distinct()
                .ToList();

            var newKeywords = allKeywords.Where(k => !tagMap.ContainsKey(k)).ToList();

            foreach (var keyword in newKeywords)
            {
                try
                {
                    var newTag = await _mealieClient.CreateTagAsync(keyword);
                    tagMap[keyword] = newTag;
                    _logger.LogInformation("Created new tag: {keyword}", keyword);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to create tag: {keyword}", keyword);
                }
            }

            return tagMap;
        }

        private async Task<Dictionary<string, MealieCategory>> PrepareCategoriesAsync(List<Recipe> recipes)
        {
            _logger.LogInformation("Preparing categories...");
        
            var existingCategories = await _mealieClient.GetCategoriesAsync();
            var categoryMap = existingCategories.Items.ToDictionary(c => c.Name, c => c);

            var allCategories = recipes
                .SelectMany(r => r.RecipeCategory)
                .Where(c => !string.IsNullOrEmpty(c))
                .Distinct()
                .ToList();

            var newCategories = allCategories.Where(c => !categoryMap.ContainsKey(c)).ToList();

            foreach (var category in newCategories)
            {
                try
                {
                    var newCategory = await _mealieClient.CreateCategoryAsync(category);
                    categoryMap[category] = newCategory;
                    _logger.LogInformation("Created new category: {category}", category);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to create category: {category}", category);
                }
            }

            return categoryMap;
        }

        private async Task<Dictionary<string, MealieTool>> PrepareToolsAsync(List<Recipe> recipes)
        {
            _logger.LogInformation("Preparing tools...");
        
            var existingTools = await _mealieClient.GetToolsAsync();
            var toolMap = existingTools.Items.ToDictionary(t => t.Name, t => t);

            var allTools = recipes
                .SelectMany(r => r.Tools)
                .Where(t => !string.IsNullOrEmpty(t))
                .Distinct()
                .ToList();

            var newTools = allTools.Where(t => !toolMap.ContainsKey(t)).ToList();

            foreach (var tool in newTools)
            {
                try
                {
                    var newTool = await _mealieClient.CreateToolAsync(tool);
                    toolMap[tool] = newTool;
                    _logger.LogInformation("Created new tool: {tool}", tool);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to create tool: {tool}", tool);
                }
            }

            return toolMap;
        }

        private async Task ImportSingleRecipeAsync(
            Recipe recipe, 
            Dictionary<string, MealieCategory> categoryMap, 
            Dictionary<string, MealieTool> toolMap, 
            BulkImportResult result)
        {
            _logger.LogInformation("Importing recipe: {recipeName}", recipe.Name);

            // Create the recipe
            var recipeName = await _mealieClient.CreateRecipeAsync(recipe);

            // Update categories
            if (recipe.RecipeCategory.Count != 0)
            {
                var categories = recipe.RecipeCategory
                    .Where(categoryMap.ContainsKey)
                    .Select(c => categoryMap[c])
                    .ToList();
            
                if (categories.Count != 0)
                {
                    await _mealieClient.UpdateRecipeCategoriesAsync(recipeName, categories);
                }
            }

            // Update tools
            if (recipe.Tools.Count != 0)
            {
                var tools = recipe.Tools
                    .Where(t => toolMap.ContainsKey(t))
                    .Select(t => toolMap[t])
                    .ToList();
            
                if (tools.Count != 0)
                {
                    await _mealieClient.UpdateRecipeToolsAsync(recipeName, tools);
                }
            }

            // Parse and update ingredients
            if (recipe.RecipeIngredient.Count != 0)
            {
                try
                {
                    var parsedIngredients = await _mealieClient.ParseIngredientsAsync(recipe.RecipeIngredient);
                
                    // Ensure food items exist
                    parsedIngredients = await EnsureFoodItemsExistAsync(parsedIngredients);
                
                    await _mealieClient.UpdateRecipeIngredientsAsync(recipeName, parsedIngredients);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to parse/update ingredients for recipe: {recipeName}", recipe.Name);
                }
            }

            result.SuccessCount++;
            result.SuccessfulRecipes.Add(recipe.Name);
            _logger.LogInformation("Successfully imported recipe: {recipeName}", recipe.Name);
        }

        private async Task<List<ParsedIngredient>> EnsureFoodItemsExistAsync(List<ParsedIngredient> ingredients)
        {
            for(var i = 0;  i < ingredients.Count; i++) {
            //foreach (var ingredient in ingredients)
            //{
                if (string.IsNullOrEmpty(ingredients[i].Ingredient.Food.Id))
                {
                    try
                    {
                        var foodId = await _mealieClient.CreateFoodAsync(ingredients[i].Ingredient.Food);
                        ingredients[i].Ingredient.Food.Id = foodId;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to create food item: {ingredientIngredientFoodName}", ingredients[i].Ingredient.Food.Name);
                    }
                }
            }
            return ingredients;
        }
    }
}