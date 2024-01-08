﻿using JarvisBot.Data;
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
        private static TelegramBotClient _botClient = new($"{TelegramBotConfiguration.LoadBotClientConfiguration()}");
        private static readonly ChatId _userChatId = new (TelegramBotConfiguration.LoadChatIdConfiguration());
        private static User _botClientUsername = new();

        private static JarvisKeyboardButtons _keyboardButtons = new();
        private static CommunicationMethods _communicationMethods = new();
        private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();


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

            _botClientUsername = await _botClient.GetMeAsync();
            Console.WriteLine($"Start listening for @{_botClientUsername.Username}");
            _logger.Info($"Start listening for @{_botClientUsername.Username}");

            using (Mutex mutex = new Mutex(true, "MyApp", out bool isNewInstance))
            {
                using var tocen = new CancellationTokenSource();

                if (!isNewInstance)
                {
                    Console.WriteLine("Программа запущена !!!");
                    return;
                }

                _botClient.SendTextMessageAsync(_userChatId, "К вашим услугам, сэр.", replyMarkup: _keyboardButtons.GetMenuButtons());

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

            await _communicationMethods.ProcessingMessage(botClient, message, _botClientUsername);           
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


        

        private static void OnProcessExit(object sender, EventArgs e)
        {
            _logger.Info("Для меня честь быть с Вами");
            _botClient.SendTextMessageAsync(_userChatId, "Для меня честь быть с Вами");
        }
    }
}
