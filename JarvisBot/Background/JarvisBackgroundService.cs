using JarvisBot.Data;
using JarvisBot.KeyboardButtons;
using Newtonsoft.Json.Linq;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot;
using Telegram.Bot.Polling;
using System.Threading;
using Microsoft.Extensions.Hosting;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types.Enums;

namespace JarvisBot.Background
{
    class JarvisBackgroundService : BackgroundService
    {

        TelegramBotClient _botClient = new($"{TelegramBotConfiguration.LoadBotClientConfiguration()}");
        private static readonly ChatId _userChatId = new(TelegramBotConfiguration.LoadChatIdConfiguration());
        private User _botClientUsername = new();

        private static JarvisKeyboardButtons _keyboardButtons = new();
        private static CommunicationMethods _communicationMethods = new();

        private ILogger _logger = LogManager.GetCurrentClassLogger();




        private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var message = update.Message;
            var callbackQuery = update.CallbackQuery;

            if (update.Type == UpdateType.CallbackQuery)
            {
                await HandleCallbackQueryAsync(botClient, callbackQuery);
                return;
            }

            if (message.Text == null)
            {
                return;
            }

            Console.WriteLine($"Отправитель - {message.Chat.FirstName}  ||  сообщение - '{message.Text}' ");
            _logger.Info($"Отправитель - {message.Chat.FirstName}  ||  сообщение - '{message.Text}' ");

            await _communicationMethods.ProcessingMessage(botClient, message, _botClientUsername);
        }

        private Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException
                    => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };
            _logger.Error(ErrorMessage);
            Console.WriteLine(ErrorMessage);
            return Task.CompletedTask;
        }

        private static async Task HandleCallbackQueryAsync(ITelegramBotClient botClient, CallbackQuery query)
        {
            await _communicationMethods.ProcessingCallback(botClient, query);
        }


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {

            while (!stoppingToken.IsCancellationRequested)
            {                
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

                    await _botClient.SendTextMessageAsync(_userChatId, "К вашим услугам, сэр.", replyMarkup: _keyboardButtons.GetMenuButtons());
                    _botClient.StartReceiving(HandleUpdateAsync, HandlePollingErrorAsync, cancellationToken: tocen.Token);

                    Console.WriteLine("Программа запущена !!!");
                    await Task.Delay(1000, stoppingToken);
                }
                return;
            }
        }


        private void OnProcessExit(object sender, EventArgs e)
        {
            _logger.Info("Для меня честь быть с Вами");
            _botClient.SendTextMessageAsync(_userChatId, "Для меня честь быть с Вами");
        }
    }
}
