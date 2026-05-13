using TranscriptAnalyzer.Models;

namespace TranscriptAnalyzer.Services;

public class TranscriptAnalyzerService
{
    private readonly List<ComplianceItem> _complianceItems =
    [
        new ComplianceItem
        {
            Name = "KVKK metni okundu",
            Keywords = ["kvkk", "kişisel veri", "aydınlatma metni", "kvkk metnini okudum"],
            Weight = 20
        },
        new ComplianceItem
        {
            Name = "Açık rıza alındı",
            Keywords = ["kabul ediyorum", "onaylıyorum", "rıza", "evet kabul", "açık rıza"],
            Weight = 20
        },
        new ComplianceItem
        {
            Name = "Risk beyanı yapıldı",
            Keywords = ["risk", "risk beyanı", "yatırım riski", "zarar edebilirsiniz", "risk bildirimi"],
            Weight = 20
        },
        new ComplianceItem
        {
            Name = "Gelir beyanı soruldu",
            Keywords = ["gelir", "maaş", "aylık kazanç", "gelir beyanı", "yıllık gelir", "gelir durumu"],
            Weight = 20
        },
        new ComplianceItem
        {
            Name = "Hesap açma amacı soruldu",
            Keywords = ["hesap açma amacı", "hesap amacı", "açma amacınız", "neden hesap", "tasarruf", "yatırım", "vadesiz", "maaş hesabı"],
            Weight = 20
        }
    ];

    public AnalysisResult Analyze(string transcript)
    {
        string filteredTranscript = string.Join(" ", transcript
            .Split('\n')
            .Where(line => !line.TrimStart().StartsWith("Müşteri:", StringComparison.OrdinalIgnoreCase)));

        string normalizedTranscript = filteredTranscript.ToLowerInvariant();

        foreach (ComplianceItem item in _complianceItems)
        {
            item.IsFound = item.Keywords.Any(k => normalizedTranscript.Contains(k.ToLowerInvariant()));
        }

        List<string> missingItems = _complianceItems
            .Where(i => !i.IsFound)
            .Select(i => i.Name)
            .ToList();

        int score = _complianceItems
            .Where(i => i.IsFound)
            .Sum(i => i.Weight);

        return new AnalysisResult
        {
            Items = [.. _complianceItems],
            MissingItems = missingItems,
            Score = score
        };
    }
}

public class AnalysisResult
{
    public List<ComplianceItem> Items { get; init; } = [];
    public List<string> MissingItems { get; init; } = [];
    public int Score { get; init; }
}
