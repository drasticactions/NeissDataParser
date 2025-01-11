using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.IO;
using System.Speech.Synthesis;

namespace NeissDataParser;

public class TTSVoice
{
    public string Name { get; set; }
    public string Culture { get; set; }
    public string Gender { get; set; }
    public string Platform { get; set; }

    public TTSVoice(string name, string culture, string gender, string platform)
    {
        Name = name;
        Culture = culture;
        Gender = gender;
        Platform = platform;
    }
}

public class CrossPlatformTTS : IDisposable
{
    private SpeechSynthesizer? windowsSynthesizer;
    private bool isInitialized = false;
    private TTSVoice? currentVoice;

    public CrossPlatformTTS()
    {
        Initialize();
    }

    private void Initialize()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            windowsSynthesizer = new SpeechSynthesizer();
            isInitialized = true;
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            try
            {
                System.Diagnostics.Process.Start("espeak", "--version");
                isInitialized = true;
            }
            catch
            {
                throw new PlatformNotSupportedException("espeak is not installed. Please install it using your package manager.");
            }
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            isInitialized = true;
        }
        else
        {
            throw new PlatformNotSupportedException("Current platform is not supported.");
        }
    }

    public async Task<List<TTSVoice>> GetAvailableVoicesAsync()
    {
        var voices = new List<TTSVoice>();

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            if (windowsSynthesizer != null)
            {
                foreach (var voice in windowsSynthesizer.GetInstalledVoices())
                {
                    var info = voice.VoiceInfo;
                    voices.Add(new TTSVoice(
                        info.Name,
                        info.Culture.Name,
                        info.Gender.ToString(),
                        "Windows"
                    ));
                }
            }
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            // Get espeak voices
            var process = await ExecuteCommandAsync("espeak", "--voices");
            var output = process.StandardOutput.ReadToEnd();
            
            foreach (var line in output.Split('\n').Skip(1)) // Skip header
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 4)
                {
                    voices.Add(new TTSVoice(
                        parts[3], // Name
                        parts[1], // Language
                        "Unknown",
                        "Linux"
                    ));
                }
            }
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            // Get macOS voices
            var process = await ExecuteCommandAsync("say", "-v ?");
            var output = process.StandardOutput.ReadToEnd();
            
            foreach (var line in output.Split('\n'))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 2)
                {
                    voices.Add(new TTSVoice(
                        parts[0], // Name
                        parts[1],
                        "Unknown",
                        "macOS"
                    ));
                }
            }
        }

        return voices;
    }

    public void SetVoice(TTSVoice voice)
    {
        if (!isInitialized)
            throw new InvalidOperationException("TTS not initialized.");

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            if (windowsSynthesizer != null)
            {
                windowsSynthesizer.SelectVoice(voice.Name);
            }
        }
        
        currentVoice = voice;
    }

    public async Task SpeakAsync(string text)
    {
        if (!isInitialized)
            throw new InvalidOperationException("TTS not initialized.");

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
#if WINDOWS
            await Task.Run(() => windowsSynthesizer?.Speak(text));
#endif
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            string voiceParam = currentVoice != null ? $"-v {currentVoice.Name}" : "";
            await ExecuteCommandAsync("espeak", $"{voiceParam} \"{text}\"");
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            string voiceParam = currentVoice != null ? $"-v {currentVoice.Name}" : "";
            await ExecuteCommandAsync("say", $"{voiceParam} \"{text}\"");
        }
    }

    public async Task SaveToFileAsync(string text, string outputPath)
    {
        if (!isInitialized)
            throw new InvalidOperationException("TTS not initialized.");

        Directory.CreateDirectory(Path.GetDirectoryName(outputPath) ?? "");

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            await Task.Run(() =>
            {
                if (windowsSynthesizer != null)
                {
                    string extension = Path.GetExtension(outputPath).ToLower();
                    if (extension != ".wav")
                    {
                        outputPath = Path.ChangeExtension(outputPath, ".wav");
                    }
                    
#if WINDOWS
                    windowsSynthesizer.SetOutputToWaveFile(outputPath);
                    windowsSynthesizer.Speak(text);
                    windowsSynthesizer.SetOutputToNull();
#endif
                }
            });
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            string extension = Path.GetExtension(outputPath).ToLower();
            if (extension != ".wav")
            {
                outputPath = Path.ChangeExtension(outputPath, ".wav");
            }
            
            string voiceParam = currentVoice != null ? $"-v {currentVoice.Name}" : "";
            await ExecuteCommandAsync("espeak", $"{voiceParam} -w \"{outputPath}\" \"{text}\"");
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            string extension = Path.GetExtension(outputPath).ToLower();
            if (extension != ".aiff")
            {
                outputPath = Path.ChangeExtension(outputPath, ".aiff");
            }
            
            string voiceParam = currentVoice != null ? $"-v {currentVoice.Name}" : "";
            await ExecuteCommandAsync("say", $"{voiceParam} -o \"{outputPath}\" \"{text}\"");
        }
    }

    private async Task<System.Diagnostics.Process> ExecuteCommandAsync(string command, string arguments)
    {
        var processInfo = new System.Diagnostics.ProcessStartInfo
        {
            FileName = command,
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        var process = new System.Diagnostics.Process();
        process.StartInfo = processInfo;
        process.Start();
        await process.WaitForExitAsync();
        return process;
    }

    public void Dispose()
    {
        #if WINDOWS
        windowsSynthesizer?.Dispose();
        #endif
    }
}