using System.Speech.Recognition;
using System.Text;

namespace TranscriptAnalyzer.Services;

public class SpeechTranscriptService
{
    private readonly SpeechRecognitionEngine _recognizer;
    private readonly StringBuilder _transcript = new();
    private string _currentSpeaker = "Agent";
    private bool _isListening;

    public SpeechTranscriptService()
    {
        _recognizer = new SpeechRecognitionEngine(new System.Globalization.CultureInfo("tr-TR"));
        _recognizer.LoadGrammar(new DictationGrammar());
        _recognizer.SetInputToDefaultAudioDevice();
        _recognizer.SpeechRecognized += OnSpeechRecognized;
    }

    public string Record()
    {
        _transcript.Clear();
        _isListening = true;

        Console.WriteLine();
        Console.WriteLine("MİKROFON MODU");
        Console.WriteLine("-------------------------------------------");
        Console.WriteLine("  [A] → Agent konuşuyor");
        Console.WriteLine("  [M] → Müşteri konuşuyor");
        Console.WriteLine("  [Enter] → Kaydı bitir ve analiz et");
        Console.WriteLine("-------------------------------------------");
        Console.WriteLine($"  Aktif: {_currentSpeaker}  (konuşabilirsiniz...)");
        Console.WriteLine();

        _recognizer.RecognizeAsync(RecognizeMode.Multiple);

        while (_isListening)
        {
            if (!Console.KeyAvailable)
            {
                Thread.Sleep(100);
                continue;
            }

            ConsoleKey key = Console.ReadKey(intercept: true).Key;

            if (key == ConsoleKey.A)
            {
                _currentSpeaker = "Agent";
                Console.WriteLine($"  [Geçiş] → Agent konuşuyor...");
            }
            else if (key == ConsoleKey.M)
            {
                _currentSpeaker = "Müşteri";
                Console.WriteLine($"  [Geçiş] → Müşteri konuşuyor...");
            }
            else if (key == ConsoleKey.Enter)
            {
                _isListening = false;
            }
        }

        _recognizer.RecognizeAsyncStop();
        return _transcript.ToString();
    }

    private void OnSpeechRecognized(object? sender, SpeechRecognizedEventArgs e)
    {
        if (!_isListening || e.Result.Confidence < 0.5f)
            return;

        string line = $"{_currentSpeaker}: {e.Result.Text}";
        _transcript.AppendLine(line);
        Console.WriteLine($"  → {line}");
    }
}
