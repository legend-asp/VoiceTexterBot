using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text;
using Telegram.Bot;
using VoiceTexterBot.Configuration;
using VoiceTexterBot.Controllers;
using VoiceTexterBot.Services;
using VoiceTexterBot1.Services;

namespace VoiceTexterBot;

public class Program
{
    public static async Task Main(string[] args)
    {
        Console.OutputEncoding = Encoding.Unicode;

        // Современный builder в .NET 8/9
        var builder = Host.CreateApplicationBuilder(args);

        // Гарантируем, что appsettings.json читается из папки сборки рядом с .exe
        builder.Configuration
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true);

        // Биндим секцию "AppSettings" из JSON в C#-класс AppSettings
        var appSettings = builder.Configuration
            .GetSection("AppSettings")
            .Get<AppSettings>()
            ?? throw new InvalidOperationException("Секция 'AppSettings' не найдена в appsettings.json!");

        builder.Services.AddSingleton(appSettings);

        // Регистрация сервисов и хранилища
        builder.Services.AddSingleton<IStorage, MemoryStorage>();
        builder.Services.AddSingleton<IFileHandler, AudioFileHandler>();

        // Контроллеры
        builder.Services.AddTransient<DefaultMessageController>();
        builder.Services.AddTransient<VoiceMessageController>();
        builder.Services.AddTransient<TextMessageController>();
        builder.Services.AddTransient<InlineKeyboardController>();

        // Telegram Client и Background Worker
        builder.Services.AddSingleton<ITelegramBotClient>(new TelegramBotClient(appSettings.BotToken));
        builder.Services.AddHostedService<Bot>();

        var host = builder.Build();

        Console.WriteLine("Сервис запущен...");
        await host.RunAsync();
    }
}