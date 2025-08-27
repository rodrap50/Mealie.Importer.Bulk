namespace API;

public class BulkImportResult
{
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public List<string> SuccessfulRecipes { get; set; } = [];
    public List<string> FailedRecipes { get; set; } = [];
    public List<string> ErrorMessages { get; set; } = [];
}
