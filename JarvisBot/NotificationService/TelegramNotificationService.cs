using JarvisBot.Background;
using JarvisBot.Core.Interfaces;
using JarvisBot.Core.Models;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;

namespace JarvisBot.NotificationService
{
    public sealed class TelegramNotificationService : INotificationService
    {
        private readonly ITelegramBotClient _botClient;
        private readonly JarvisClientSettings _settings;


        public TelegramNotificationService(ITelegramBotClient botClient, JarvisClientSettings settings)
        {
            _botClient = botClient;
            _settings = settings;
        }


        public async Task NotifyAsync(WatchTask task, MonitoringResult result, CancellationToken cancellationToken = default)
        {
            var message =
                $"🔔 Условие сработало!\n\n" +
                $"Задача: {task.Name}\n" +
                $"URL: {task.Url}\n" +
                $"Условие: {task.ConditionValue}";

            await _botClient.SendMessage(_settings.AdminChatId, message, cancellationToken: cancellationToken);
        }
    }
}
