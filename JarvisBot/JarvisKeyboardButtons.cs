using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types.ReplyMarkups;

namespace JarvisBot
{
    public class JarvisKeyboardButtons
    {


        public IReplyMarkup GetMenuButtons()
        {
            ReplyKeyboardMarkup replyKeyboard = new(new[]
                {
                    new KeyboardButton[] {"Help", "Курсы валют"},
                    new KeyboardButton[] {"Help", "Курсы валют"},
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
                    new KeyboardButton[] {"US", "RUS"},
                    new KeyboardButton[] {"EUR", "CHN"},
                })
            {
                ResizeKeyboard = true
            };
            return replyKeyboard;
        }
    }
}
