namespace TranscriptAnalyzer.Models;

public class ComplianceItem
{
    public string Name { get; init; } = string.Empty;
    public string[] Keywords { get; init; } = [];
    public bool IsFound { get; set; }
    public int Weight { get; init; }
}
