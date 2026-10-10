namespace VoiceTexterBot1.Services;

public interface IFileHandler
{
    Task<string> Download(string fileId, CancellationToken ct);
    string Process(string inputAudioPath, string languageCode);
}