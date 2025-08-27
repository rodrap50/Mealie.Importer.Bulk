namespace API.Infrastructure.Services;
using Models;

public interface IMealieConfigProvider
{
    MealieConfig GetConfig(string? requestBaseUrl = null, string? requestApiToken = null);
}
