using System.Text.Json.Serialization;

namespace API.Infrastructure.Models;

public class NutritionInformation
{
    [JsonPropertyName("@type")]
    public string? Type { get; set; }
    public string? Calories { get; set; }
    public string? FatContent { get; set; }
    public string? CarbohydrateContent { get; set; }
    public string? ProteinContent { get; set; }
}
