namespace JarvisBot.Background
{
    public class JarvisClientSettings
    {
        public string TelegramBotClient { get; set; } = string.Empty;
        public string AdminChatIdString { get; set; } = string.Empty;
        public int AdminChatId { get; set; }
        public string YandexKey { get; set; } = string.Empty;
        public string AlfabankRate { get; set; } = string.Empty;
        public string YandexWeather { get; set; } = string.Empty;

        public string OldExchangeRatesPath { get; set; } = string.Empty;
    }
}
