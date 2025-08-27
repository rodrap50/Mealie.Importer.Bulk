namespace API.Infrastructure.Models;

public class MealieTool
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? GroupId { get; set; }
    public string? Slug { get; set; }
    public List<string> HouseholdsWithTool { get; set; } = [];
}
