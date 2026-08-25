using JarvisBot.Core.Models;
using System.Collections.Generic;
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
                    new KeyboardButton[] { "😎 Ассистент" }
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
                    new KeyboardButton[] { "😎 Ассистент", "🙋‍♂️ Help" }
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
                    new KeyboardButton[] { "📋 Задачи дня", "📋 Мониторинги" },
                    new KeyboardButton[] { "🍔 Меню - Балука", "⬅️ Back" }
                })
            {
                ResizeKeyboard = true
            };
            return replyKeyboard;
        }

        public ReplyMarkup GetMonitoringMenuButtons()
        {
            ReplyKeyboardMarkup replyKeyboard = new(new[]
                {
                    new KeyboardButton[] { "➕ Добавить мониторинг", "⏯️ Остановить мониторинг" },
                    new KeyboardButton[] { "⏩ Запустить мониторинг", "➖ Удалить мониторинг" },
                    new KeyboardButton[] { "⬅️ Back" }
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

        public ReplyMarkup GetStopMonitoringButtons(List<WatchTask> tasks)
        {
            var buttons = new List<InlineKeyboardButton[]>();

            foreach (var task in tasks)
            {
                buttons.Add(new[]
                {
                    InlineKeyboardButton.WithCallbackData(text: $"🟢 {task.Name}", callbackData: $"monitor_stop:{task.Id}")
                });
            }
            return new InlineKeyboardMarkup(buttons);
        }

        public ReplyMarkup GetStartMonitoringButtons(List<WatchTask> tasks)
        {
            var buttons = new List<InlineKeyboardButton[]>();

            foreach (var task in tasks)
            {
                buttons.Add(new[]
                {
                    InlineKeyboardButton.WithCallbackData(
                        text: $"⏩ {task.Name}",
                        callbackData: $"monitor_start:{task.Id}")
                });
            }

            return new InlineKeyboardMarkup(buttons);
        }

        public ReplyMarkup GetDeleteMonitoringButtons(List<WatchTask> tasks)
        {
            var buttons = new List<InlineKeyboardButton[]>();

            foreach (var task in tasks)
            {
                buttons.Add(new[]
                {
                    InlineKeyboardButton.WithCallbackData(
                        text: $"➖ {task.Name}",
                        callbackData: $"monitor_delete:{task.Id}")
                });
            }

            return new InlineKeyboardMarkup(buttons);
        }
    }
}
