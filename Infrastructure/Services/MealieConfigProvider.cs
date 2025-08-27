namespace API.Infrastructure.Services;

using Microsoft.Extensions.Options;
using Models;

public class MealieConfigProvider(IOptions<MealieConfig> config) : IMealieConfigProvider
{
    private readonly MealieConfig _baseConfig = config.Value;

    public MealieConfig GetConfig(string? requestBaseUrl = null, string? requestApiToken = null)
    {
        return new MealieConfig
        {
            BaseUrl = requestBaseUrl ?? _baseConfig.BaseUrl,
            ApiToken = requestApiToken ?? _baseConfig.ApiToken
        };
    }
}
