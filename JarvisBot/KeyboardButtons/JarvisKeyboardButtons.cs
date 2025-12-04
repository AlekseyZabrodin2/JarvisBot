using Telegram.Bot.Types.ReplyMarkups;

namespace JarvisBot.KeyboardButtons
{
    public class JarvisKeyboardButtons
    {


        public ReplyMarkup GetMenuButtons()
        {
            ReplyKeyboardMarkup replyKeyboard = new(new[]
                {
                    new KeyboardButton[] { "☂️ Погода", "💰 Курсы валют" },
                    new KeyboardButton[] { "📋 Задачи" }
                })
            {
                ResizeKeyboard = true
            };
            return replyKeyboard;
        }

        public ReplyMarkup GetAdminMenuButtons()
        {
            ReplyKeyboardMarkup replyKeyboard = new(new[]
                {
                    new KeyboardButton[] { "☂️ Погода", "💰 Курсы валют" },
                    new KeyboardButton[] { "📋 Задачи", "🙋‍♂️ Help" }
                })
            {
                ResizeKeyboard = true
            };
            return replyKeyboard;
        }

        public ReplyMarkup GetTasksMenuButtons()
        {
            ReplyKeyboardMarkup replyKeyboard = new(new[]
                {
                    new KeyboardButton[] { "📋 На сегодня", "📅 На неделю" },
                    new KeyboardButton[] {  "⬅️ Back" }
                })
            {
                ResizeKeyboard = true
            };
            return replyKeyboard;
        }

        public ReplyMarkup GetHelpButtons()
        {
            ReplyKeyboardMarkup replyKeyboard = new(new[]
                {
                    new KeyboardButton[] { "🙋‍♂️ Help" }
                })
            {
                ResizeKeyboard = true
            };
            return replyKeyboard;
        }

        public ReplyMarkup GetBackButtons()
        {
            ReplyKeyboardMarkup replyKeyboard = new(new[]
                {
                    new KeyboardButton[] { "⬅️ Back" }
                })
            {
                ResizeKeyboard = true
            };
            return replyKeyboard;
        }

        public ReplyMarkup GetMoneyButtons()
        {
            ReplyKeyboardMarkup replyKeyboard = new(new[]
                {
                    new KeyboardButton[] { "💵 USD", "💷 RUB" },
                    new KeyboardButton[] { "💶 EUR", "Auto 🔄️" },
                    new KeyboardButton[] {  "⬅️ Back" },
                })
            {
                ResizeKeyboard = true
            };
            return replyKeyboard;
        }

        public ReplyMarkup GetAutoRateButtons()
        {
            ReplyKeyboardMarkup replyKeyboard = new(new[]
                {
                    new KeyboardButton[] { "Auto 💵 💷 💶", "Stop 🔄️" },
                    new KeyboardButton[] {  "⬅️ Back" },
                })
            {
                ResizeKeyboard = true
            };
            return replyKeyboard;
        }

        public ReplyMarkup GetHelpSubmenuButtons()
        {
            ReplyKeyboardMarkup replyKeyboard = new(new[]
                {
                    new KeyboardButton[] { "💻 Device", "🛠️ Something" },
                    new KeyboardButton[] { "⬅️ Back" }
                })
            {
                ResizeKeyboard = true
            };
            return replyKeyboard;
        }

        public ReplyMarkup GetStartAnyDeskButtons()
        {
            InlineKeyboardMarkup inlineKeyboard = new(new[]
            {
                new []
                {
                    InlineKeyboardButton.WithCallbackData(text: "🚀 Start AnyDesk", callbackData: "Start_AnyDesk"),
                    InlineKeyboardButton.WithCallbackData(text: "🛑 Cancel AnyDesk", callbackData: "Cancel_AnyDesk"),
                }
            });

            return inlineKeyboard;
        }

        public ReplyMarkup GetRebootButtons()
        {
            InlineKeyboardMarkup inlineKeyboard = new(new[]
            {
                new []
                {
                    InlineKeyboardButton.WithCallbackData(text: "🔐 Lock PC", callbackData: "PC_Lock"),
                    InlineKeyboardButton.WithCallbackData(text: "♻️ Reboot", callbackData: "PC_Reboot"),
                    InlineKeyboardButton.WithCallbackData(text: "💡 Power OFF", callbackData: "PC_PowerOFF")
                }
            });

            return inlineKeyboard;
        }


    }
}
