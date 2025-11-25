using JarvisBot.KeyboardButtons;
using Microsoft.Extensions.Hosting;
using NLog;
using System;
using System.Net.Http;
using System.Net.Sockets;
using System.Text.Json;
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

        TelegramBotClient _botClient;
        private readonly ChatId _adminChatId;
        private User _botClientUsername = new();
        private readonly JarvisClientSettings _clientSettings;
        private static JarvisKeyboardButtons _keyboardButtons = new();
        private static CommunicationMethods _communicationMethods;
        private bool _connectionLost = false;
        private bool _secondRequestException = false;

        private ILogger _logger = LogManager.GetCurrentClassLogger();


        public JarvisBackgroundService(JarvisClientSettings clientSettings, CommunicationMethods communicationMethods)
        {
            _clientSettings = clientSettings;
            _communicationMethods = communicationMethods;

            _botClient = new($"{_clientSettings.TelegramBotClient}");
            _adminChatId = new(_clientSettings.AdminChatId); 
        }


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.Info("Sending connection requests");

            AppDomain.CurrentDomain.ProcessExit += OnProcessExit;

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var tocen = new CancellationTokenSource();

                    _botClientUsername = await _botClient.GetMe();
                    Console.WriteLine($"Start listening for @{_botClientUsername.Username}");
                    _logger.Info($"Start listening for @{_botClientUsername.Username}");

                    await _botClient.SendMessage(_adminChatId, "К вашим услугам, сэр.", replyMarkup: _keyboardButtons.GetMenuButtons());

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
            if (_connectionLost)
            {
                _logger.Info("Connection restored!");
                _connectionLost = false;
            }

            var message = update.Message;
            var callbackQuery = update.CallbackQuery;

            if (update.Type == UpdateType.CallbackQuery)
            {
                await _communicationMethods.ProcessingCallback(botClient, callbackQuery, _botClientUsername);
                return;
            }

            if (message?.Text == null)
            {
                return;
            }

            WriteRequestToBotMessage(message);

            await _communicationMethods.ProcessingMessage(botClient, message, _botClientUsername, cancellationToken);
        }

        private Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var errorMessage = exception switch
            {
                ApiRequestException apiRequestException when apiRequestException.ErrorCode == 429 =>
                    $"Rate limit exceeded. Retry after {apiRequestException.Parameters?.RetryAfter ?? 0} seconds.",
                ApiRequestException apiRequestException when apiRequestException.ErrorCode == 401 =>
                    "Unauthorized: Invalid Bot Token or Bot blocked.",
                ApiRequestException apiRequestException =>
                    $"Telegram API Error {apiRequestException.ErrorCode}: {apiRequestException.Message}",
                HttpRequestException httpRequestException =>
                    $"Network error: {httpRequestException.Message}",
                SocketException socketException =>
                    $"Socket error: {socketException.Message}",
                TaskCanceledException =>
                    "Request timed out.",
                JsonException jsonException =>
                    $"JSON deserialization error: {jsonException.Message}",
                RequestException requestException =>
                    $"Request exception: {requestException.Message}",
                Exception ex =>
                    $"Unexpected error: {ex.GetType().Name} - {ex.Message}",

                _ => exception.ToString()
            };
            if (!_connectionLost)
            {
                _logger.Error($"Error in HandlePollingErrorAsync - {errorMessage}");
                Console.WriteLine(errorMessage);
                if (exception is RequestException)
                {
                    if (_secondRequestException)
                    {
                        _connectionLost = true;
                        _secondRequestException = false;
                    }
                    _secondRequestException = true;
                }
            }  
            if (!(exception is RequestException))
            {
                if (_connectionLost)
                {
                    _connectionLost = false;
                    _logger.Error($"Error in HandlePollingErrorAsync, but not RequestException - {errorMessage}");
                }
            }

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
            _logger.Info("До скорого, для меня честь быть с Вами");
            _botClient.SendMessage(_adminChatId, "До скорого, для меня честь быть с Вами");
        }
    }
}
