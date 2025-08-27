namespace API.Infrastructure.Models;

public class MealieIngredient
{
    public MealieFood Food { get; set; } = new();
    public string? Quantity { get; set; }
    public string? Unit { get; set; }
    public string? Note { get; set; }
}
