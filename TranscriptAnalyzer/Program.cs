using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TranscriptAnalyzer.Mcp;
using TranscriptAnalyzer.Services;

// MCP Server modu: dotnet run --mcp
if (args.Contains("--mcp"))
{
    var builder = Host.CreateApplicationBuilder(args);
    builder.Services
        .AddMcpServer()
        .WithStdioServerTransport()
        .WithToolsFromAssembly(typeof(TranscriptAnalyzerTool).Assembly);

    await builder.Build().RunAsync();
    return;
}

Console.OutputEncoding = System.Text.Encoding.UTF8;

Console.WriteLine("===========================================");
Console.WriteLine("       TRANSKRIPT UYUM ANALİZİ            ");
Console.WriteLine("===========================================");
Console.WriteLine();
Console.WriteLine("  [1] Mikrofon ile kaydet");
Console.WriteLine("  [2] Örnek transkript ile test et");
Console.WriteLine();
Console.Write("Seçiminiz: ");
string? choice = Console.ReadLine();

string transcript;
if (choice == "1")
{
    var speechService = new SpeechTranscriptService();
    transcript = speechService.Record();

    if (string.IsNullOrWhiteSpace(transcript))
    {
        Console.WriteLine("Transkript alınamadı.");
        return;
    }
}
else
{
    transcript = @"
Agent: KVKK metnini okudum.

Müşteri: Evet kabul ediyorum.

Agent: Hesap açma amacınız nedir?

Müşteri: Tasarruf.
";
}

string? githubToken = Environment.GetEnvironmentVariable("GITHUB_TOKEN");

AnalysisResult result;
if (!string.IsNullOrEmpty(githubToken))
{
    var llmService = new LlmTranscriptAnalyzerService(githubToken);
    result = await llmService.AnalyzeAsync(transcript);
}
else
{
    var service = new TranscriptAnalyzerService();
    result = service.Analyze(transcript);
}


Console.WriteLine();
Console.WriteLine("===========================================");
Console.WriteLine("       TRANSKRIPT UYUM ANALİZİ            ");
Console.WriteLine("===========================================");
Console.WriteLine();

Console.WriteLine("KONTROL LİSTESİ:");
Console.WriteLine("-------------------------------------------");
foreach (var item in result.Items)
{
    string status = item.IsFound ? "[✓]" : "[✗]";
    Console.WriteLine($"  {status} {item.Name} ({item.Weight} puan)");
}

Console.WriteLine();

if (result.MissingItems.Count > 0)
{
    Console.WriteLine("EKSİK MADDELER:");
    Console.WriteLine("-------------------------------------------");
    foreach (string missing in result.MissingItems)
    {
        Console.WriteLine($"  • {missing}");
    }
    Console.WriteLine();
}
else
{
    Console.WriteLine("Tüm maddeler tamamlandı.");
    Console.WriteLine();
}

string scoreLabel = result.Score switch
{
    100 => "Mükemmel",
    >= 80 => "İyi",
    >= 60 => "Orta",
    >= 40 => "Zayıf",
    _ => "Yetersiz"
};

Console.WriteLine($"UYUM SKORU: {result.Score}/100  →  {scoreLabel}");
Console.WriteLine("===========================================");
