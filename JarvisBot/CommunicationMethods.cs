using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot;

namespace JarvisBot
{
    public class CommunicationMethods
    {
        private static JarvisKeyboardButtons _keyboardButtons = new();

        public async Task HandleGreetingAsync(ITelegramBotClient botClient, Message message)
        {
            if (message.Text.Contains("Привет", StringComparison.CurrentCultureIgnoreCase))
            {
                await botClient.SendTextMessageAsync(message.Chat.Id, "Privet");
            }
        }

        public async Task HandleMenuAsync(ITelegramBotClient botClient, Message message)
        {
            if (message.Text.Contains("Меню", StringComparison.CurrentCultureIgnoreCase) ||
                message.Text.Contains("Menu", StringComparison.CurrentCultureIgnoreCase))
            {
                await botClient.SendTextMessageAsync(message.Chat.Id, text: "Choose", replyMarkup: _keyboardButtons.GetMenuButtons());
            }
        }

        public async Task HandleCurrencyAsync(ITelegramBotClient botClient, Message message)
        {
            if (message.Text.Contains("Курсы валют", StringComparison.CurrentCultureIgnoreCase))
            {
                await botClient.SendTextMessageAsync(message.Chat.Id, text: "Выберите валюту", replyMarkup: _keyboardButtons.GetMoneyButtons());
            }
        }

    }
}
