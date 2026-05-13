using System.ComponentModel;
using ModelContextProtocol.Server;
using TranscriptAnalyzer.Services;

namespace TranscriptAnalyzer.Mcp;

[McpServerToolType]
public static class TranscriptAnalyzerTool
{
    [McpServerTool]
    [Description("Banka agent-müşteri görüşme transkriptini uyum açısından analiz eder. Agentın KVKK, açık rıza, risk beyanı, gelir beyanı ve hesap açma amacı maddelerini yerine getirip getirmediğini kontrol eder.")]
    public static string AnalyzeTranscript(
        [Description("Agent ve müşteri arasındaki görüşme metni. Her satır 'Agent: ...' veya 'Müşteri: ...' formatında olmalıdır.")] string transcript)
    {
        var service = new TranscriptAnalyzerService();
        var result = service.Analyze(transcript);

        var lines = new List<string>();
        lines.Add($"UYUM SKORU: {result.Score}/100");
        lines.Add("");
        lines.Add("KONTROL LİSTESİ:");
        foreach (var item in result.Items)
        {
            string status = item.IsFound ? "✓" : "✗";
            lines.Add($"  [{status}] {item.Name} ({item.Weight} puan)");
        }

        if (result.MissingItems.Count > 0)
        {
            lines.Add("");
            lines.Add("EKSİK MADDELER:");
            foreach (string missing in result.MissingItems)
                lines.Add($"  • {missing}");
        }

        string scoreLabel = result.Score switch
        {
            100 => "Mükemmel",
            >= 80 => "İyi",
            >= 60 => "Orta",
            >= 40 => "Zayıf",
            _ => "Yetersiz"
        };
        lines.Add("");
        lines.Add($"SONUÇ: {scoreLabel}");

        return string.Join("\n", lines);
    }
}
