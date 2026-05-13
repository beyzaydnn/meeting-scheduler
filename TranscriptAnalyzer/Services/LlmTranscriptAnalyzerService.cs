using Azure;
using Azure.AI.Inference;
using System.Text.Json;
using TranscriptAnalyzer.Models;

namespace TranscriptAnalyzer.Services;

public class LlmTranscriptAnalyzerService
{
    private readonly ChatCompletionsClient _client;
    private readonly string _model = "gpt-4o-mini";

    private readonly List<ComplianceItem> _complianceItems =
    [
        new ComplianceItem { Name = "KVKK metni okundu",         Weight = 20 },
        new ComplianceItem { Name = "Açık rıza alındı",          Weight = 20 },
        new ComplianceItem { Name = "Risk beyanı yapıldı",       Weight = 20 },
        new ComplianceItem { Name = "Gelir beyanı soruldu",      Weight = 20 },
        new ComplianceItem { Name = "Hesap açma amacı soruldu",  Weight = 20 }
    ];

    public LlmTranscriptAnalyzerService(string githubToken)
    {
        _client = new ChatCompletionsClient(
            new Uri("https://models.inference.ai.azure.com"),
            new AzureKeyCredential(githubToken));
    }

    public async Task<AnalysisResult> AnalyzeAsync(string transcript)
    {
        string filteredTranscript = string.Join("\n", transcript
            .Split('\n')
            .Where(line => !line.TrimStart().StartsWith("Müşteri:", StringComparison.OrdinalIgnoreCase)));

        string itemList = string.Join("\n", _complianceItems.Select((item, i) => $"{i + 1}. {item.Name}"));

        string prompt = $$"""
            Aşağıdaki banka agent konuşmasını analiz et ve her uyum maddesinin geçip geçmediğini belirle.

            AGENT KONUŞMASI:
            {{filteredTranscript}}

            UYUM MADDELERİ:
            {{itemList}}

            Her madde için yalnızca true veya false döndür. Yanıtı şu JSON formatında ver:
            {"results": [true, false, true, false, true]}

            Açıklama ekleme, sadece JSON döndür.
            """;

        ChatCompletionsOptions options = new()
        {
            Model = _model,
            Messages = { new ChatRequestUserMessage(prompt) }
        };

        Response<ChatCompletions> response = await _client.CompleteAsync(options);
        string content = response.Value.Content;

        bool[] results = ParseResults(content, _complianceItems.Count);

        for (int i = 0; i < _complianceItems.Count; i++)
            _complianceItems[i].IsFound = results[i];

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

    private static bool[] ParseResults(string content, int count)
    {
        try
        {
            string json = content.Trim();
            if (json.Contains("```"))
                json = json.Split("```")[1].Replace("json", "").Trim();

            using JsonDocument doc = JsonDocument.Parse(json);
            JsonElement resultsArray = doc.RootElement.GetProperty("results");
            return [.. resultsArray.EnumerateArray().Select(e => e.GetBoolean())];
        }
        catch
        {
            return new bool[count];
        }
    }
}
