using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
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

        public IReplyMarkup GetHelpButtons()
        {
            ReplyKeyboardMarkup replyKeyboard = new(new[]
                {
                    new KeyboardButton[] {"Device", "Something"},
                    new KeyboardButton[] {"< Back"}
                })
            {
                ResizeKeyboard = true
            };
            return replyKeyboard;
        }

        public IReplyMarkup GetStartAnyDeskButtons()
        {
            InlineKeyboardMarkup inlineKeyboard = new(new[]
            {
                new []
                {
                    InlineKeyboardButton.WithCallbackData(text: "Start AnyDesk", callbackData: "Start_AnyDesk"),
                    InlineKeyboardButton.WithCallbackData(text: "Cancel AnyDesk", callbackData: "Cancel_AnyDesk"),
                }
            });

            return inlineKeyboard;
        }

        public IReplyMarkup GetRebootButtons()
        {
            InlineKeyboardMarkup inlineKeyboard = new(new[]
            {
                new []
                {
                    InlineKeyboardButton.WithCallbackData(text: "Reboot", callbackData: "PC_Reboot"),
                    InlineKeyboardButton.WithCallbackData(text: "Power OFF", callbackData: "PC_PowerOFF"),
                }
            });

            return inlineKeyboard;
        }


    }
}
