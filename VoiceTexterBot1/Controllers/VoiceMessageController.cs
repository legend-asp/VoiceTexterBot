using Telegram.Bot;
using Telegram.Bot.Types;
using VoiceTexterBot.Services;
using VoiceTexterBot1.Services;

namespace VoiceTexterBot.Controllers
{
    public class VoiceMessageController
    {
        private readonly IStorage _memoryStorage; // Добавим это
        private readonly ITelegramBotClient _telegramClient;
        private readonly IFileHandler _audioFileHandler;

        public VoiceMessageController(ITelegramBotClient telegramBotClient, IFileHandler audioFileHandler, IStorage memoryStorage)
        {
            _telegramClient = telegramBotClient;
            _audioFileHandler = audioFileHandler;
            _memoryStorage = memoryStorage; // и это
        }

        public async Task Handle(Message message, CancellationToken ct)
        {
            var fileId = message.Voice?.FileId;
            if (fileId == null)
                return;

            // 1. Скачиваем файл и сохраняем его уникальный путь
            string filePath = await _audioFileHandler.Download(fileId, ct);

            // 2. Получаем язык пользователя из сессии
            string userLanguageCode = _memoryStorage.GetSession(message.Chat.Id).LanguageCode;

            // 3. Передаем путь к файлу и язык в обработчик
            var result = _audioFileHandler.Process(filePath, userLanguageCode);

            await _telegramClient.SendMessage(message.Chat.Id, result, cancellationToken: ct);
        }
    }
}