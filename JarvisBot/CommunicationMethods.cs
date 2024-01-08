using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot;
using NLog;

namespace JarvisBot
{
    public class CommunicationMethods
    {

        private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();
        private static JarvisKeyboardButtons _keyboardButtons = new();
        private static Message _botMessage = new();


        public async Task ProcessingMessage(ITelegramBotClient botClient, Message message, User botUsername)
        {
            await HandleGreetingAsync(botClient, message);
            await HandleMenuAsync(botClient, message);
            await HandleCurrencyAsync(botClient, message);

            WriteBotMessage(botUsername, _botMessage);
        }

        public async Task HandleGreetingAsync(ITelegramBotClient botClient, Message message)
        {
            if (message.Text.Contains("Привет", StringComparison.CurrentCultureIgnoreCase))
            {
                _botMessage = await botClient.SendTextMessageAsync(message.Chat.Id, "Privet");

            }
        }

        public async Task HandleMenuAsync(ITelegramBotClient botClient, Message message)
        {
            if (message.Text.Contains("Меню", StringComparison.CurrentCultureIgnoreCase) ||
                message.Text.Contains("Menu", StringComparison.CurrentCultureIgnoreCase))
            {
                _botMessage = await botClient.SendTextMessageAsync(message.Chat.Id, text: "Choose", replyMarkup: _keyboardButtons.GetMenuButtons());
            }
        }

        public async Task HandleCurrencyAsync(ITelegramBotClient botClient, Message message)
        {
            if (message.Text.Contains("Курсы валют", StringComparison.CurrentCultureIgnoreCase))
            {
                _botMessage = await botClient.SendTextMessageAsync(message.Chat.Id, text: "Выберите валюту", replyMarkup: _keyboardButtons.GetMoneyButtons());
            }
        }


        private static void WriteBotMessage(User botUsername, Message message)
        {

            Console.WriteLine($"Ответ - {botUsername.Username}  ||  сообщение - '{message.Text}' ");
            _logger.Info($"Ответ - {botUsername.Username}  ||  сообщение - '{message.Text}' ");
            _botMessage.Text = string.Empty;
        }
    }
}
