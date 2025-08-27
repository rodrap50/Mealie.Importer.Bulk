using System;
using System.Text.Json.Serialization;

namespace API.Infrastructure.Models;

public class Recipe
{
    [JsonPropertyName("@context")]
    public string? Context { get; set; }
    
    [JsonPropertyName("@type")]
    public string? Type { get; set; }
    
    public string Name { get; set; } = string.Empty;
    public string? Image { get; set; }
    public string? Url { get; set; }
    public string? Description { get; set; }
    public List<string> RecipeIngredient { get; set; } = new();
    public List<RecipeInstruction> RecipeInstructions { get; set; } = new();
    public List<string> RecipeCategory { get; set; } = new();
    public List<string> Tools { get; set; } = new();
    public string? RecipeYield { get; set; }
    public string? PrepTime { get; set; }
    public string? CookTime { get; set; }
    public List<string> Keywords { get; set; } = new();
    public NutritionInformation? Nutrition { get; set; }
}
