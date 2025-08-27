namespace API.Infrastructure.Middleware;

using System.Text.Json;
using Microsoft.Extensions.Options;
using Models;
public class MealieConfigValidationMiddleware(
    RequestDelegate next,
    IOptions<MealieConfig> defaultConfig,
    ILogger<MealieConfigValidationMiddleware> logger)
{
    private readonly RequestDelegate _next = next;
    private readonly MealieConfig _defaultConfig = defaultConfig.Value;
    private readonly ILogger<MealieConfigValidationMiddleware> _logger = logger;

    public async Task InvokeAsync(HttpContext context)
    {
        // Only validate on bulk import endpoints
        if (context.Request.Path.StartsWithSegments("/api/bulkimport"))
        {
            var config = GetMealieConfig(context);
            var validationResult = ValidateConfig(config);
            
            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Mealie configuration validation failed: {Errors}", 
                    string.Join(", ", validationResult.Errors));
                    
                await WriteErrorResponse(context, validationResult);
                return;
            }
            
            // Store validated config for controller use
            context.Items["MealieConfig"] = config;
            _logger.LogDebug("Mealie configuration validated successfully for {BaseUrl}", config.BaseUrl);
        }

        await _next(context);
    }

    private MealieConfig GetMealieConfig(HttpContext context)
    {
        // Priority order: Headers -> Environment Variables -> Default Config
        var baseUrl = context.Request.Headers["X-Mealie-Base-Url"].FirstOrDefault()
                     ?? _defaultConfig.BaseUrl;

        var apiToken = context.Request.Headers["X-Mealie-Api-Token"].FirstOrDefault()
                      ?? _defaultConfig.ApiToken;

        return new MealieConfig
        {
            BaseUrl = baseUrl?.Trim() ?? string.Empty,
            ApiToken = apiToken?.Trim() ?? string.Empty
        };
    }

    private static ValidationResult ValidateConfig(MealieConfig config)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(config.BaseUrl))
        {
            errors.Add("Mealie Base URL is required. Provide it via X-Mealie-Base-Url header, MEALIE_BASE_URL environment variable, or appsettings.json");
        }
        else if (!Uri.TryCreate(config.BaseUrl, UriKind.Absolute, out var uri) || 
                 (uri.Scheme != "http" && uri.Scheme != "https"))
        {
            errors.Add("Mealie Base URL must be a valid HTTP or HTTPS URL");
        }

        if (string.IsNullOrWhiteSpace(config.ApiToken))
        {
            errors.Add("Mealie API Token is required. Provide it via X-Mealie-Api-Token header, MEALIE_API_TOKEN environment variable, or appsettings.json");
        }
        else if (config.ApiToken.Length < 10) // Basic token length validation
        {
            errors.Add("Mealie API Token appears to be invalid (too short)");
        }

        return new ValidationResult
        {
            IsValid = errors.Count == 0,
            Errors = errors
        };
    }

    private static async Task WriteErrorResponse(HttpContext context, ValidationResult validationResult)
    {
        context.Response.StatusCode = 400;
        context.Response.ContentType = "application/json";

        var errorResponse = new
        {
            message = "Mealie configuration validation failed",
            errors = validationResult.Errors,
            configurationSources = new
            {
                headers = new[] { "X-Mealie-Base-Url", "X-Mealie-Api-Token" },
                environmentVariables = new[] { "MEALIE_BASE_URL", "MEALIE_API_TOKEN" },
                configFile = "appsettings.json -> Mealie section"
            }
        };

        var json = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        });

        await context.Response.WriteAsync(json);
    }

    private class ValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = [];
    }
}
