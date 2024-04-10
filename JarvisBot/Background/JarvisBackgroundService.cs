using JarvisBot.Data;
using JarvisBot.KeyboardButtons;
using Microsoft.Extensions.Hosting;
using NLog;
using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace JarvisBot.Background
{
    class JarvisBackgroundService : BackgroundService
    {

        TelegramBotClient _botClient = new($"{TelegramBotConfiguration.LoadBotClientConfiguration()}");
        private static readonly ChatId _adminChatId = new(TelegramBotConfiguration.LoadChatIdConfiguration());
        private User _botClientUsername = new();

        private static JarvisKeyboardButtons _keyboardButtons = new();
        private static CommunicationMethods _communicationMethods = new();

        private ILogger _logger = LogManager.GetCurrentClassLogger();



        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.Info("Sending connection requests");

            AppDomain.CurrentDomain.ProcessExit += OnProcessExit;

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var tocen = new CancellationTokenSource();

                    _botClientUsername = await _botClient.GetMeAsync();
                    Console.WriteLine($"Start listening for @{_botClientUsername.Username}");
                    _logger.Info($"Start listening for @{_botClientUsername.Username}");

                    await _botClient.SendTextMessageAsync(_adminChatId, "К вашим услугам, сэр.", replyMarkup: _keyboardButtons.GetMenuButtons());

                    _botClient.StartReceiving(HandleUpdateAsync, HandlePollingErrorAsync, cancellationToken: tocen.Token);

                    _logger.Info("Request was sent successfully, the connection is established");
                    Console.WriteLine("Программа запущена !!!");
                    await Task.Delay(1000, stoppingToken);
                    return;
                }
                catch (Exception exeption)
                {
                    _logger.Error($"Jarvis don`t ExecuteAsync - {exeption}");
                    await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
                }
            }
        }


        private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var message = update.Message;
            var callbackQuery = update.CallbackQuery;

            if (update.Type == UpdateType.CallbackQuery)
            {
                await _communicationMethods.ProcessingCallback(botClient, callbackQuery, _botClientUsername);
                return;
            }

            if (message.Text == null)
            {
                return;
            }

            WriteRequestToBotMessage(message);

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
            _logger.Error($"Error in HandlePollingErrorAsync - {ErrorMessage}");
            Console.WriteLine(ErrorMessage);
            return Task.CompletedTask;
        }

        private void WriteRequestToBotMessage(Message message)
        {
            if (message.Chat.Username == null)
            {
                Console.WriteLine($"Отправитель --> {message.Chat.FirstName} {message.Chat.LastName}({message.Chat.Id}) ||  сообщение - '{message.Text}' ");
                _logger.Info($"Отправитель --> {message.Chat.FirstName} {message.Chat.LastName}({message.Chat.Id})  ||  сообщение - '{message.Text}' ");
            }
            else
            {
                Console.WriteLine($"Отправитель --> {message.Chat.Username}({message.Chat.Id}) ||  сообщение - '{message.Text}' ");
                _logger.Info($"Отправитель --> {message.Chat.Username}({message.Chat.Id})  ||  сообщение - '{message.Text}' ");
            }
        }

        private void OnProcessExit(object sender, EventArgs e)
        {
            _logger.Info("Для меня честь быть с Вами");
            _botClient.SendTextMessageAsync(_adminChatId, "Для меня честь быть с Вами");
        }
    }
}
