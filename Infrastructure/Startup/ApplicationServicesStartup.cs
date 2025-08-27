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

        builder.Services.Configure<JsonSerializerOptions>(options =>
        {
            options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            options.WriteIndented = true;
        });

        // Add configuration
        builder.Services.Configure<MealieConfig>(
        builder.Configuration.GetSection("Mealie"));
        
        return builder;
    }
}
