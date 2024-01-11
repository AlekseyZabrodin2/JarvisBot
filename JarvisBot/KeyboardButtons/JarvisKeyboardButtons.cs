using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types.ReplyMarkups;

namespace JarvisBot.KeyboardButtons
{
    public class JarvisKeyboardButtons
    {


        public IReplyMarkup GetMenuButtons()
        {
            ReplyKeyboardMarkup replyKeyboard = new(new[]
                {
                    new KeyboardButton[] {"Help", "Курсы валют"},
                    new KeyboardButton[] {"Help", "Погода"},
                })
            {
                ResizeKeyboard = true
            };
            return replyKeyboard;
        }

        public IReplyMarkup GetMoneyButtons()
        {
            ReplyKeyboardMarkup replyKeyboard = new(new[]
                {
                    new KeyboardButton[] {"USD", "RUB"},
                    new KeyboardButton[] {"EUR", "< Back"},
                })
            {
                ResizeKeyboard = true
            };
            return replyKeyboard;
        }
    }
}
