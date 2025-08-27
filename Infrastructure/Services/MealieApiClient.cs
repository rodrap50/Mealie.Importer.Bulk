namespace API.Infrastructure.Services;

using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Models;
using Models.Response;

public class MealieApiClient : IMealieApiClient
{
    private readonly HttpClient _httpClient;
    private readonly MealieConfig _config;
    private readonly ILogger<MealieApiClient> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public MealieApiClient(HttpClient httpClient, IOptions<MealieConfig> config, ILogger<MealieApiClient> logger)
    {
        _httpClient = httpClient;
        _config = config.Value;
        _logger = logger;
        
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };

        _httpClient.BaseAddress = new Uri(_config.BaseUrl);
        _httpClient.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _config.ApiToken);
    }

    public async Task<MealieItemsResponse<MealieTag>> GetTagsAsync()
    {
        var response = await _httpClient.GetAsync("api/organizers/tags");
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<MealieItemsResponse<MealieTag>>(content, _jsonOptions) ?? new();
    }

    public async Task<MealieTag> CreateTagAsync(string name)
    {
        var tag = new { name };
        var json = JsonSerializer.Serialize(tag, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        var response = await _httpClient.PostAsync("api/organizers/tags", content);
        response.EnsureSuccessStatusCode();
        
        var responseContent = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<MealieTag>(responseContent, _jsonOptions) ?? new();
    }

    public async Task<MealieItemsResponse<MealieCategory>> GetCategoriesAsync()
    {
        var response = await _httpClient.GetAsync("api/organizers/categories");
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<MealieItemsResponse<MealieCategory>>(content, _jsonOptions) ?? new();
    }

    public async Task<MealieCategory> CreateCategoryAsync(string name)
    {
        var category = new { name };
        var json = JsonSerializer.Serialize(category, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        var response = await _httpClient.PostAsync("api/organizers/categories", content);
        response.EnsureSuccessStatusCode();
        
        var responseContent = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<MealieCategory>(responseContent, _jsonOptions) ?? new();
    }

    public async Task<MealieItemsResponse<MealieTool>> GetToolsAsync()
    {
        var response = await _httpClient.GetAsync("api/organizers/tools");
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<MealieItemsResponse<MealieTool>>(content, _jsonOptions) ?? new();
    }

    public async Task<MealieTool> CreateToolAsync(string name)
    {
        var tool = new { name, householdsWithTool = new List<string>() };
        var json = JsonSerializer.Serialize(tool, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        var response = await _httpClient.PostAsync("api/organizers/tools", content);
        response.EnsureSuccessStatusCode();
        
        var responseContent = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<MealieTool>(responseContent, _jsonOptions) ?? new();
    }

    public async Task<string> CreateRecipeAsync(Recipe recipe)
    {
        var recipeData = new
        {
            includeTags = true,
            data = JsonSerializer.Serialize(recipe, _jsonOptions)
        };
        
        var json = JsonSerializer.Serialize(recipeData, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        var response = await _httpClient.PostAsync("api/recipes/create/html-or-json", content);
        response.EnsureSuccessStatusCode();
        
        var responseContent = await response.Content.ReadAsStringAsync();
        return responseContent.Trim('"'); // Remove quotes from response
    }

    public async Task<List<ParsedIngredient>> ParseIngredientsAsync(List<string> ingredients)
    {
        var data = new { parser = "nlp", ingredients };
        var json = JsonSerializer.Serialize(data, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        var response = await _httpClient.PostAsync("api/parser/ingredients", content);
        response.EnsureSuccessStatusCode();
        
        var responseContent = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<List<ParsedIngredient>>(responseContent, _jsonOptions) ?? new();
    }

    public async Task UpdateRecipeIngredientsAsync(string recipeName, List<ParsedIngredient> ingredients)
    {
        var ingredientList = ingredients.Select(i => i.Ingredient).ToList();
        var data = new { recipeIngredient = ingredientList };
        var json = JsonSerializer.Serialize(data, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        var response = await _httpClient.PatchAsync($"api/recipes/{recipeName}", content);
        response.EnsureSuccessStatusCode();
    }

    public async Task UpdateRecipeCategoriesAsync(string recipeName, List<MealieCategory> categories)
    {
        var data = new { recipeCategory = categories };
        var json = JsonSerializer.Serialize(data, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        var response = await _httpClient.PatchAsync($"api/recipes/{recipeName}", content);
        response.EnsureSuccessStatusCode();
    }

    public async Task UpdateRecipeToolsAsync(string recipeName, List<MealieTool> tools)
    {
        var data = new { tools };
        var json = JsonSerializer.Serialize(data, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        var response = await _httpClient.PatchAsync($"api/recipes/{recipeName}", content);
        response.EnsureSuccessStatusCode();
    }

    public async Task<string> CreateFoodAsync(MealieFood food)
    {
        var json = JsonSerializer.Serialize(food, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        var response = await _httpClient.PostAsync("api/foods", content);
        response.EnsureSuccessStatusCode();
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var createdFood = JsonSerializer.Deserialize<MealieFood>(responseContent, _jsonOptions);
        return createdFood?.Id ?? string.Empty;
    }
}

