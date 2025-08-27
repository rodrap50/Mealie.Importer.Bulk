namespace API.Infrastructure.Models;

public class MealieTag
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? GroupId { get; set; }
    public string? Slug { get; set; }
}