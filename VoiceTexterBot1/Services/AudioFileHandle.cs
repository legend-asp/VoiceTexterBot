using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using VoiceTexterBot.Configuration;
using VoiceTexterBot.Utilities;
using VoiceTexterBot1.Services;

namespace VoiceTexterBot.Services
{
    public class AudioFileHandler : IFileHandler
    {
        private readonly AppSettings _appSettings;
        private readonly ITelegramBotClient _telegramBotClient;
        private readonly ILogger<AudioFileHandler> _logger;
        private readonly string _downloadsFolder;

        public AudioFileHandler(
            ITelegramBotClient telegramBotClient,
            AppSettings appSettings,
            ILogger<AudioFileHandler> logger)
        {
            _telegramBotClient = telegramBotClient ?? throw new ArgumentNullException(nameof(telegramBotClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            // Используем папку AppData если нет прав на запись в Downloads
            _downloadsFolder = GetWritableDownloadsFolder(appSettings);
            _appSettings = appSettings;
        }

        private string GetWritableDownloadsFolder(AppSettings appSettings)
        {
            try
            {
                // Проверяем доступность стандартной папки
                Directory.CreateDirectory(appSettings.DownloadsFolder);
                using (File.Create(Path.Combine(appSettings.DownloadsFolder, "test.tmp"))) { }
                File.Delete(Path.Combine(appSettings.DownloadsFolder, "test.tmp"));
                return appSettings.DownloadsFolder;
            }
            catch
            {
                // Если нет прав, используем LocalApplicationData
                var appDataPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "VoiceTexterBot",
                    "Downloads");

                Directory.CreateDirectory(appDataPath);
                return appDataPath;
            }
        }

        public async Task Download(string fileId, CancellationToken ct)
        {
            string inputAudioFilePath = Path.Combine(
                _downloadsFolder,
                $"{_appSettings.AudioFileName}.{_appSettings.InputAudioFormat}");

            try
            {
                _logger.LogInformation($"Downloading to: {inputAudioFilePath}");

                using (var destinationStream = File.Create(inputAudioFilePath))
                {
                    var file = await _telegramBotClient.GetFile(fileId, ct);
                    if (file.FilePath == null)
                        throw new InvalidOperationException("File path is null");

                    await _telegramBotClient.DownloadFile(file.FilePath, destinationStream, ct);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Download failed");
                throw;
            }
        }

        public string Process(string languageCode)
        {
            string inputAudioPath = Path.Combine(_appSettings.DownloadsFolder, $"{_appSettings.AudioFileName}.{_appSettings.InputAudioFormat}");
            string outputAudioPath = Path.Combine(_appSettings.DownloadsFolder, $"{_appSettings.AudioFileName}.{_appSettings.OutputAudioFormat}");

            Console.WriteLine("Начинаем конвертацию...");
            AudioConverter.TryConvert(inputAudioPath, outputAudioPath);
            Console.WriteLine("Файл конвертирован");

            Console.WriteLine("Начинаем распознавание...");
            var speechText = SpeechDetector.DetectSpeech(outputAudioPath, _appSettings.InputAudioBitrate, languageCode);
            Console.WriteLine("Файл распознан.");
            return speechText;
        }
    }
}