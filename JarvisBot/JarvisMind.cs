using JarvisBot.Data;
using NLog;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace JarvisBot
{
    class JarvisMind
    {
        private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

        private static TelegramBotClient _botClient = new($"{TelegramBotConfiguration.LoadBotClientConfiguration()}");
        private static readonly ChatId _userChatId = new (TelegramBotConfiguration.LoadChatIdConfiguration());
        


        //[DllImport("kernel32.dll")]
        //static extern IntPtr GetConsoleWindow();

        //[DllImport("user32.dll")]
        //static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        //const int SW_HIDE = 0;  

        private static async Task Main()
        {

            //IntPtr consoleHandle = GetConsoleWindow();
            //ShowWindow(consoleHandle, SW_HIDE);

            AppDomain.CurrentDomain.ProcessExit += OnProcessExit;

            var me = await _botClient.GetMeAsync();
            Console.WriteLine($"Start listening for @{me.Username}");

            using (Mutex mutex = new Mutex(true, "MyApp", out bool isNewInstance))
            {
                using var tocen = new CancellationTokenSource();

                if (!isNewInstance)
                {
                    Console.WriteLine("Программа запущена !!!");
                    return;
                }

                _botClient.SendTextMessageAsync(_userChatId, "К вашим услугам, сэр.", replyMarkup: GetButtons());

                _botClient.StartReceiving(HandleUpdateAsync, HandlePollingErrorAsync, cancellationToken: tocen.Token);
                
                Console.ReadLine();
                tocen.Cancel();
            }

        }

        private static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var message = update.Message;

            Console.WriteLine($"Отправитель - {message.Chat.FirstName}  ||  сообщение - '{message.Text}' ");
            _logger.Info($"Отправитель - {message.Chat.FirstName}  ||  сообщение - '{message.Text}' ");


            if (message.Text == null)
            {
                return;
            }

            await HandleGreetingAsync(botClient, message);
            await HandleMenuAsync(botClient, message);
            await HandleCurrencyAsync(botClient, message);
        }

        private static Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException
                    => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            Console.WriteLine(ErrorMessage);
            return Task.CompletedTask;
        }


        private static async Task HandleGreetingAsync(ITelegramBotClient botClient, Message message)
        {
            if (message.Text.Contains("Привет", StringComparison.CurrentCultureIgnoreCase))
            {
                await botClient.SendTextMessageAsync(message.Chat.Id, "Privet");
            }
        }

        private static async Task HandleMenuAsync(ITelegramBotClient botClient, Message message)
        {
            if (message.Text.Contains("Меню", StringComparison.CurrentCultureIgnoreCase) ||
                message.Text.Contains("Menu", StringComparison.CurrentCultureIgnoreCase))
            {
                await botClient.SendTextMessageAsync(message.Chat.Id, text: "Choose", replyMarkup: GetButtons());
            }
        }

        private static async Task HandleCurrencyAsync(ITelegramBotClient botClient, Message message)
        {
            if (message.Text.Contains("Курсы валют", StringComparison.CurrentCultureIgnoreCase))
            {
                await botClient.SendTextMessageAsync(message.Chat.Id, text: "Выберите валюту", replyMarkup: GetMoneyButtons());
            }
        }





        private static IReplyMarkup GetButtons()
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

        private static IReplyMarkup GetMoneyButtons()
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




        private static void OnProcessExit(object sender, EventArgs e)
        {
            _logger.Info("Для меня честь быть с Вами");
            _botClient.SendTextMessageAsync(_userChatId, "Для меня честь быть с Вами");
        }



    }
}
