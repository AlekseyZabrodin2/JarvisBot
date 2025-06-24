using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JarvisBot.Background
{
    public class JarvisClientSettings
    {
        public string? TelegramBotClient { get; set; }
        public int AdminChatId { get; set; }
        public string? YandexKey { get; set; }
        public string? AlfabankRate { get; set; }
        public string? YandexWeather { get; set; }

        public string? OldExchangeRatesPath { get; set; }
    }
}
