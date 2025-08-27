namespace API.Infrastructure.Startup;

using System.Text.Json;
using Services;
using Models;
public static class ApplicationServicesStartup
{
    public static WebApplicationBuilder AddCustomServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        builder.Services.AddHttpClient<IMealieApiClient, MealieApiClient>();

        builder.Services.AddScoped<IMealieApiClient, MealieApiClient>();
        builder.Services.AddScoped<IBulkImportService, BulkImportService>();
        builder.Services.AddScoped<IMealieConfigProvider, MealieConfigProvider>();

        builder.Services.Configure<JsonSerializerOptions>(options =>
        {
            options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            options.WriteIndented = true;
        });

        // Add configuration
        builder.Services.Configure<MealieConfig>(config => {
            // First try appsettings
            builder.Configuration.GetSection("Mealie").Bind(config);
            
            // Override with environment variables if they exist
            var envBaseUrl = builder.Configuration.GetValue<string>("MEALIE_BASE_URL");
            var envApiToken = builder.Configuration.GetValue<string>("MEALIE_API_TOKEN");
            
            if (!string.IsNullOrEmpty(envBaseUrl))
                config.BaseUrl = envBaseUrl;
            
            if (!string.IsNullOrEmpty(envApiToken))
                config.ApiToken = envApiToken;
        });
        
        return builder;
    }
}
