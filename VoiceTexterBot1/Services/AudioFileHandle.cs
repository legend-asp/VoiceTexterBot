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

        public async Task<string> Download(string fileId, CancellationToken ct)
        {
            // Генерируем уникальный путь под каждый запрос
            string uniqueFileName = $"{Guid.NewGuid()}.{_appSettings.InputAudioFormat}";
            string inputAudioFilePath = Path.Combine(_downloadsFolder, uniqueFileName);

            try
            {
                _logger.LogInformation($"Скачиваем аудио в: {inputAudioFilePath}");

                await using var destinationStream = File.Create(inputAudioFilePath);
                var file = await _telegramBotClient.GetFile(fileId, ct);

                if (file.FilePath == null)
                    throw new InvalidOperationException("Файл в Telegram не найден");

                await _telegramBotClient.DownloadFile(file.FilePath, destinationStream, ct);

                return inputAudioFilePath; // Возвращаем уникальный путь!
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка скачивания файла");
                // Если скачивание упало, подчищаем недокачанный огрызок
                if (File.Exists(inputAudioFilePath))
                    File.Delete(inputAudioFilePath);

                throw;
            }
        }

        public string Process(string inputAudioPath, string languageCode)
        {
            // Автоматически получаем путь к .wav: "xyz.ogg" -> "xyz.wav"
            string outputAudioPath = Path.ChangeExtension(inputAudioPath, _appSettings.OutputAudioFormat);

            try
            {
                _logger.LogInformation("Конвертация в WAV...");
                AudioConverter.TryConvert(inputAudioPath, outputAudioPath);

                _logger.LogInformation("Распознавание речи...");
                var speechText = SpeechDetector.DetectSpeech(outputAudioPath, _appSettings.InputAudioBitrate, languageCode);

                return speechText;
            }
            finally
            {
                // 🧹 Блок finally выполнится ВСЕГДА — даже если произойдет ошибка или сбой!
                // Удаляем оба временных файла, чтобы не засорять диск
                if (File.Exists(inputAudioPath)) File.Delete(inputAudioPath);
                if (File.Exists(outputAudioPath)) File.Delete(outputAudioPath);

                _logger.LogInformation("Временные файлы удалены.");
            }
        }
    }
}