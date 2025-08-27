using System.Text.Json.Serialization;

namespace API.Infrastructure.Models;

public class RecipeInstruction
{
    [JsonPropertyName("@type")]
    public string? Type { get; set; }
    public string Text { get; set; } = string.Empty;
}
