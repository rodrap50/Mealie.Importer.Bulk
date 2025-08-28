namespace API.Infrastructure.Models;

public class MealieIngredient
{
    public MealieFood Food { get; set; } = new();
    public double Quantity { get; set; }
    public Dictionary<string, object> Unit { get; set; } = new();
    public string? Note { get; set; }
}
