namespace API.Infrastructure.Models.Response;

public class MealieItemsResponse<T>
{
    public List<T> Items { get; set; } = new();
    public int Total { get; set; }
    public int Page { get; set; }
    public int PerPage { get; set; }
}
