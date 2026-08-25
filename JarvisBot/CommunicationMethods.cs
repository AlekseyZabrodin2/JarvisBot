using Grpc.Net.Client;
using JarvisBot.Background;
using JarvisBot.Core.Enums;
using JarvisBot.Core.Interfaces;
using JarvisBot.Core.Models;
using JarvisBot.Engine.Monitoring;
using JarvisBot.Exchange.AlfaBankInSyncRates;
using JarvisBot.KeyboardButtons;
using JarvisBot.Models;
using JarvisBot.TasksFromGrpc;
using JarvisBot.Weather;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramBotGrpcService;

namespace JarvisBot
{
    public class CommunicationMethods
    {
        private readonly JarvisClientSettings _clientSettings;
        private readonly ChatId _adminChatId;
        private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();
        private static JarvisKeyboardButtons _keyboardButtons = new();
        private static Message _botMessage = new();
        private ITelegramBotClient _botClient;
        private static ExchangeRateLoder _exchangeRateLoder;
        private static WeatherLoder _weatherLoder;
        private Process? _anyDeskProcess;
        private CancellationTokenSource _cancellationToken;
        private TimerManager _timerManager = new();
        private TelegramService.TelegramServiceClient _grpcClient;
        private GrpcConnectingSettings _grpcConnectingSettings;
        private bool _messageInProcess;
        IServiceProvider _serviceProvider; 
        private readonly Dictionary<long, MonitoringCreationState> _monitoringCreationStates = new();


        public CommunicationMethods(JarvisClientSettings clientSettings, ExchangeRateLoder exchangeRateLoder, 
            WeatherLoder weatherLoder, IServiceProvider serviceProvider)
        {
            _clientSettings = clientSettings;
            _exchangeRateLoder = exchangeRateLoder;
            _weatherLoder = weatherLoder;

            _adminChatId = new(_clientSettings.AdminChatId);

            StartGrpcClient(serviceProvider);
        }



        private void StartGrpcClient(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            var options = _serviceProvider.GetService<IOptions<GrpcConnectingSettings>>();
            _grpcConnectingSettings = options.Value;

            if (_grpcConnectingSettings != null)
            {
                _grpcClient = new TelegramService.TelegramServiceClient(GrpcChannel.ForAddress(_grpcConnectingSettings.GrpcChannel));
                _logger.Info($"gRPC client started with - [{_grpcConnectingSettings.GrpcChannel}] address");
            }
            else
            {
                _logger.Error($"gRPC client not started, address is - [{_grpcConnectingSettings.GrpcChannel}]");
            }
        }

        public async Task ProcessingMessage(ITelegramBotClient botClient, Message message,
            User botUsername, CancellationToken cancellationToken)
        {
            _botClient = botClient;
            try
            {
                await HandleGreetingAsync(botClient, message);
                await HandleMenuAsync(botClient, message);
                await HandleCurrencyAsync(botClient, message);
                await HandleRatesAsync(botClient, message);
                await HandleAutoRatesAsync(botClient, message);
                await HandleStartStopAutoRatesAsync(botClient, message);
                await HandleWeatherAsync(botClient, message);
                await HandleBackToMenuAsync(botClient, message);

                await HandleHelpButtonAsync(botClient, message);
                await HandleDeviceButtonAsync(botClient, message);

                await HandleRebootButtonAsync(botClient, message);
                await HandleChooseTasksButtonAsync(botClient, message);

                await HandleGetTasksForTodayAsync(botClient, message);
                await HandleGetTaskForWeekAsync(botClient, message);
                await HandleGetMenuFromBalukAsync(botClient, message);

                await HandleMonitoringTasksAsync(botClient, message, cancellationToken);
                await HandleStartMonitoringAsync(botClient, message, cancellationToken);
                await HandleStopMonitoringAsync(botClient, message, cancellationToken);
                await HandleDeleteMonitoringAsync(botClient, message, cancellationToken);

                var monitoringStarted = await HandleAddMonitoringAsync(botClient, message);

                if (!monitoringStarted)
                {
                    await HandleMonitoringCreationAsync(botClient, message, cancellationToken);
                }

                if (string.IsNullOrEmpty(_botMessage.Text) && !_monitoringCreationStates.ContainsKey(message.Chat.Id))
                {
                    //await HandleUnknownMessageAsync(botClient, message);
                }

                WriteAnswerInBotConsole(botUsername, _botMessage);
            }
            catch (Telegram.Bot.Exceptions.ApiRequestException ex) when (ex.ErrorCode == 403)
            {
                _logger.Warn($"Пользователь {_botClient.BotId} заблокировал бота. Удаляю из списка.");
                // Можно исключить этого пользователя из рассылок
            }
            catch (Exception ex)
            {
                _logger.Error($"Неизвестная ошибка при отправке сообщения пользователю {_botClient.BotId}: {ex.Message}");
            }            
        }

        public async Task ProcessingCallback(ITelegramBotClient botClient, CallbackQuery query, User botUsername, CancellationToken cancellationToken)
        {
            await HandleStartAnyDeskAsync(botClient, query);
            await HandleRebootPCAsync(botClient, query);

            await HandleMonitorStopCallbackQueryAsync(botClient, query, cancellationToken); 
            await HandleMonitorStartCallbackQueryAsync(botClient, query);
            await HandleMonitorDeleteCallbackQueryAsync(botClient, query);

            WriteAnswerInBotConsole(botUsername, _botMessage);
        }


        private static void WriteAnswerInBotConsole(User botUsername, Message message)
        {
            if (message.Chat.Username == null)
            {
                Console.WriteLine($"Ответ - {botUsername.Username} --> {message.Chat.FirstName} {message.Chat.LastName}({message.Chat.Id})) || сообщение - '{message.Text}' ");
                _logger.Info($"Ответ - {botUsername.Username} --> {message.Chat.FirstName} {message.Chat.LastName}({message.Chat.Id})) || сообщение - '{message.Text}' ");
            }
            else
            {
                Console.WriteLine($"Ответ - {botUsername.Username} --> {message.Chat.Username}({message.Chat.Id})) || сообщение - '{message.Text}' ");
                _logger.Info($"Ответ - {botUsername.Username} --> {message.Chat.Username}({message.Chat.Id})) || сообщение - '{message.Text}' ");
            }

            _botMessage.Text = string.Empty;
        }

        public async Task HandleGreetingAsync(ITelegramBotClient botClient, Message message)
        {
            if (message.Text.Contains("Привет", StringComparison.CurrentCultureIgnoreCase))
            {
                _botMessage = await botClient.SendMessage(message.Chat.Id, "Privet");
            }
        }

        public async Task HandleBackToMenuAsync(ITelegramBotClient botClient, Message message)
        {
            if (message.Text == "⬅️ Back")
            {
                if (message.Text == "⬅️ Back" && message.Chat.Id == _adminChatId)
                {
                    _botMessage = await botClient.SendMessage(message.Chat.Id, "Вы в МЕНЮ",
                        replyMarkup: _keyboardButtons.GetAdminMenuButtons());
                }
                else
                {
                    _botMessage = await botClient.SendMessage(message.Chat.Id, "Вы в МЕНЮ",
                        replyMarkup: _keyboardButtons.GetMenuButtons());
                }
            }
        }

        public async Task HandleMenuAsync(ITelegramBotClient botClient, Message message)
        {
            if (message.Text.Contains("Меню", StringComparison.CurrentCultureIgnoreCase) ||
                message.Text.Contains("Menu", StringComparison.CurrentCultureIgnoreCase))
            {
                if (message.Text.Contains("Меню", StringComparison.CurrentCultureIgnoreCase) ||
                    message.Text.Contains("Menu", StringComparison.CurrentCultureIgnoreCase) && message.Chat.Id == _adminChatId)
                {
                    _botMessage = await botClient.SendMessage(message.Chat.Id, text: "Choose",
                        replyMarkup: _keyboardButtons.GetAdminMenuButtons());
                }
                else
                {
                    _botMessage = await botClient.SendMessage(message.Chat.Id, text: "Choose",
                        replyMarkup: _keyboardButtons.GetMenuButtons());
                }
            }
        }

        public async Task HandleCurrencyAsync(ITelegramBotClient botClient, Message message)
        {
            if (message.Text.Contains("Курсы валют", StringComparison.CurrentCultureIgnoreCase))
            {
                _botMessage = await botClient.SendMessage(message.Chat.Id, text: "Выберите валюту",
                    replyMarkup: _keyboardButtons.GetMoneyButtons());
            }
        }

        public async Task HandleRatesAsync(ITelegramBotClient botClient, Message message)
        {
            try
            {
                _cancellationToken = new();

                if (message.Text == "💵 USD" || message.Text == "💶 EUR" || message.Text == "💷 RUB")
                {
                    _cancellationToken.Cancel();

                    _messageInProcess = true;
                    var rateMessage = await _exchangeRateLoder.RatesResponse(message.Text, _cancellationToken.Token);
                    _botMessage = await botClient.SendMessage(message.Chat.Id, rateMessage);
                    _messageInProcess = false;
                    _cancellationToken = new();
                }
            }
            catch (OperationCanceledException ex)
            {
                _logger.Error($"The operation was canceled in HandleRatesAsync - [{ex}]");
            }
        }

        public async Task HandleAutoRatesAsync(ITelegramBotClient botClient, Message message)
        {
            try
            {
                _cancellationToken = new();

                if (message.Text == "Auto 🔄️")
                {
                    _botMessage = await botClient.SendMessage(message.Chat.Id, text: "Авто обновление курса валют ...",
                        replyMarkup: _keyboardButtons.GetAutoRateButtons());
                }
            }
            catch (OperationCanceledException ex)
            {
                _logger.Error($"The operation was canceled in HandleRatesAsync - [{ex}]");
            }
        }

        public async Task HandleStartStopAutoRatesAsync(ITelegramBotClient botClient, Message message)
        {
            try
            {
                _cancellationToken = new();

                if (message.Text == "Auto 💵 💷 💶")
                {
                    _botMessage = await botClient.SendMessage(message.Chat.Id, text: "Обновление Курса валют ВКЛЮЧЕНО",
                        replyMarkup: _keyboardButtons.GetAdminMenuButtons());

                    SetTimer(_cancellationToken.Token);
                }
                else if (message.Text == "Stop 🔄️")
                {
                    _botMessage = await botClient.SendMessage(message.Chat.Id, text: "Авто обновление ВЫКЛЮЧЕНО",
                        replyMarkup: _keyboardButtons.GetMoneyButtons());
                    _cancellationToken.Cancel();
                }
            }
            catch (OperationCanceledException ex)
            {
                _logger.Error($"The operation was canceled in HandleRatesAsync - [{ex}]");
            }
        }

        private void SetTimer(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                _messageInProcess = true;
                _timerManager.StopTimer("Timer1");
                _logger.Debug("Timer is stopped because triggered token");
                return;
            }

            _timerManager.StopTimer("Timer1");
            _logger.Debug("Previously started timers have been stopped");


            if (_timerManager.Timer == null || !_timerManager.TimerId.Contains("Timer1"))
            {
                _timerManager.CreateTimer("Timer1", 30);
            }

            _timerManager.Timer!.Elapsed -= OnTimedEvent;

            _logger.Trace($"New Timer {_timerManager.TimerId} is started ");
            _timerManager.Timer.Elapsed += OnTimedEvent;

            if (_timerManager.Timer.Enabled)
            {
                _timerManager.StopTimer("Timer1");
            }
            _timerManager.StartTimer("Timer1");
            _logger.Trace("Timer is started in SetTimer method");
        }

        private async void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            try
            {
                CancellationToken cancellationToken = _cancellationToken.Token;

                if (_messageInProcess)
                {
                    _logger.Info("Message in process return from method OnTimedEvent!");
                    return;
                }
                _logger.Trace("Rate updating");
                _timerManager.StopTimer("Timer1");
                _logger.Debug("Timer is stopped because rates is updating");

                var currencies = new List<string> { "💵 USD", "💶 EUR", "💷 RUB" };
                string? updateRate = null;

                cancellationToken.ThrowIfCancellationRequested();

                foreach (string rate in currencies)
                {
                    _logger.Trace($"In Rate updating");

                    if (!_messageInProcess && _timerManager.Timer.Enabled is false)
                    {
                        _messageInProcess = true;

                        _logger.Trace($"Rate for update - {rate}");
                        updateRate = await _exchangeRateLoder.EqualityCurrencyExchangeRate(rate, cancellationToken);

                        await Task.Delay(10000, cancellationToken);

                        if (updateRate != null)
                        {
                            HandleUpdateRatesAsync(_botClient, _botMessage, updateRate);
                        }
                        _logger.Trace("Rate has not been updated");
                        _messageInProcess = false;
                    }
                }
                _timerManager.StartTimer("Timer1");
                _logger.Trace("Timer is started after updating \r\n");
            }
            catch (OperationCanceledException ex)
            {
                _logger.Error($"The operation was canceled in OnTimedEvent");
            }
        }

        public async Task HandleUpdateRatesAsync(ITelegramBotClient botClient, Message message, string rateMessage)
        {
            _logger.Info($"Rate is updating - {rateMessage}");
            _botMessage = await botClient.SendMessage(message.Chat.Id, rateMessage);
        }

        public async Task HandleWeatherAsync(ITelegramBotClient botClient, Message message)
        {
            if (message.Text == "☂️ Погода")
            {
                var weatuerMessage = _weatherLoder.WeatherResponse();
                _botMessage = await botClient.SendMessage(message.Chat.Id, await weatuerMessage);
            }
        }

        public async Task HandleHelpButtonAsync(ITelegramBotClient botClient, Message message)
        {
            if (message.Text.Contains("🙋‍♂️ Help", StringComparison.CurrentCultureIgnoreCase) && message.Chat.Id == _adminChatId)
            {
                _botMessage = await botClient.SendMessage(message.Chat.Id, text: "Что-то включить?",
                    replyMarkup: _keyboardButtons.GetHelpSubmenuButtons());
            }
        }

        public async Task HandleDeviceButtonAsync(ITelegramBotClient botClient, Message message)
        {
            if (message.Text == "💻 Device" && message.Chat.Id == _adminChatId)
            {
                _botMessage = await botClient.SendMessage(message.Chat.Id, text: "Вы в меню управления программой - [AnyDesk]",
                    replyMarkup: _keyboardButtons.GetStartAnyDeskButtons());

                _botMessage = await botClient.SendMessage(message.Chat.Id, text: "Что сделать с программой, \nСэр?",
                    replyMarkup: _keyboardButtons.GetBackButtons());
            }
        }

        public async Task HandleStartAnyDeskAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery)
        {
            if (callbackQuery.Data == "Start_AnyDesk")
            {
                _botMessage = await botClient.SendMessage(_botMessage.Chat.Id, "AnyDesk включается...");
                StartAnyDesk(botClient, _botMessage);
            }
            else if (callbackQuery.Data == "Cancel_AnyDesk")
            {
                _botMessage = await botClient.SendMessage(_botMessage.Chat.Id, "Выключение AnyDesk.", replyMarkup: _keyboardButtons.GetAdminMenuButtons());
                await StopAnyDesk(botClient, _botMessage);
            }
        }

        public async void StartAnyDesk(ITelegramBotClient botClient, Message message)
        {
            if (!Process.GetProcessesByName("AnyDesk").Any())
            {
                string processName = @"C:\Program Files (x86)\AnyDesk\AnyDesk.exe";

                _anyDeskProcess = new()
                {
                    StartInfo =
                    {
                        FileName = processName,
                        Verb = "runas" // "runas" указывает на запуск с правами администратора
                    }
                };

                try
                {
                    _anyDeskProcess.Start();

                    if (Process.GetProcessesByName("AnyDesk").Any())
                    {
                        _botMessage = await botClient.SendMessage(message.Chat.Id, "AnyDesk запущен");
                        Console.WriteLine("AnyDesk запущен");
                    }
                    else
                    {
                        _botMessage = await botClient.SendMessage(message.Chat.Id, "Проблемы с запуском...");
                    }
                }
                catch (System.ComponentModel.Win32Exception ex)
                {
                    Console.WriteLine($"Ошибка: {ex.Message}");
                }
            }
            else
            {
                botClient.SendMessage(message.Chat.Id, "AnyDesk уже запущен");
                Console.WriteLine("AnyDesk уже запущен");
            }
        }

        public async Task<bool> StopAnyDesk(ITelegramBotClient botClient, Message message)
        {
            if (_anyDeskProcess != null)
            {
                await CloseAnyDeskProcesses(botClient, message);
            }

            await Task.Delay(1000);

            if (!Process.GetProcessesByName("AnyDesk").Any())
            {
                _botMessage = await botClient.SendMessage(message.Chat.Id, "AnyDesk закрыт");
                Console.WriteLine("AnyDesk закрыт");
            }
            else
            {
                _botMessage = await botClient.SendMessage(message.Chat.Id, "... закрываем AnyDesk повторно");
                Console.WriteLine("... закрываем AnyDesk повторно");

                await CloseAnyDeskProcesses(botClient, message);

                _botMessage = await botClient.SendMessage(message.Chat.Id, "AnyDesk закрыт");
                Console.WriteLine("AnyDesk закрыт");
            }

            return true;
        }

        private async Task CloseAnyDeskProcesses(ITelegramBotClient botClient, Message message)
        {
            foreach (var process in Process.GetProcessesByName("AnyDesk"))
            {
                process.Kill();

                _botMessage = await botClient.SendMessage(message.Chat.Id, "AnyDesk закрывается...");
                Console.WriteLine("AnyDesk закрывается...");
            }
        }

        public async Task HandleRebootButtonAsync(ITelegramBotClient botClient, Message message)
        {
            if (message.Text == "🛠️ Something" && message.Chat.Id == _adminChatId)
            {
                _botMessage = await botClient.SendMessage(message.Chat.Id, text: "ВНИМАНИЕ !!! \r\nВы вошли в настройки управления компьютером:",
                    replyMarkup: _keyboardButtons.GetRebootButtons());

                _botMessage = await botClient.SendMessage(message.Chat.Id, text: "Еще не поздно вернуться назад, \nСэр.",
                    replyMarkup: _keyboardButtons.GetBackButtons());
            }
        }

        public async Task HandleRebootPCAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery)
        {
            var chatId = _botMessage.Chat?.Id ?? 552523783;

            if (callbackQuery.Data == "PC_Reboot")
            {
                _botMessage = await botClient.SendMessage(chatId, "Ждите компьютер ПЕРЕЗАГРУЖАЕТСЯ...");
                RebootPcClick(botClient, _botMessage);
            }
            else if (callbackQuery.Data == "PC_PowerOFF")
            {
                _botMessage = await botClient.SendMessage(chatId, "ВЫКЛЮЧЕНИЕ компьютера...");
                PowerOffPcClick(botClient, _botMessage);
            }
            else if (callbackQuery.Data == "PC_Lock")
            {
                _botMessage = await botClient.SendMessage(chatId, "Выключение экрана ...");
                LockPcClick(botClient, _botMessage);
            }
        }

        public async void RebootPcClick(ITelegramBotClient botClient, Message message)
        {
            _botMessage = await botClient.SendMessage(message.Chat.Id, "Ждите Я скоро ..!");
            Console.WriteLine("Ждите Я скоро ..!");

            string rebootPC = "shutdown";
            string arguments = "/r /t 1";
            Process.Start(rebootPC, arguments);
        }

        public async void PowerOffPcClick(ITelegramBotClient botClient, Message message)
        {
            _botMessage = await botClient.SendMessage(message.Chat.Id, "До скорого, \nСэр");
            Console.WriteLine("До скорого, Сэр");

            string powerOffPC = "shutdown";
            string arguments = "/s /f /t 0";
            Process.Start(powerOffPC, arguments);
        }

        public async void LockPcClick(ITelegramBotClient botClient, Message message)
        {
            _botMessage = await botClient.SendMessage(message.Chat.Id, "Экран выключен, сер");
            Console.WriteLine("Экран выключен, сер");

            string lockPC = "Rundll32.exe";
            string arguments = "user32.dll,LockWorkStation";
            //Process.Start(lockPC, arguments);

            // Создана задача "LockPC" в планировщике заданий для запуска
            Process.Start(new ProcessStartInfo
            {
                FileName = "schtasks",
                Arguments = "/run /tn \"LockPC\"",
                UseShellExecute = false,
                CreateNoWindow = true
            });
        }

        public async Task HandleGetTasksForTodayAsync(ITelegramBotClient botClient, Message message)
        {
            if (message.Text == "📋 Задачи дня")
            {
                var emptyRequest = new TelegramEmptyRequest();
                var tasks = await _grpcClient.TelegramGetTasksForTodayAsync(emptyRequest);

                if (tasks.Tasks.Count == 0)
                {
                    _botMessage = await botClient.SendMessage(message.Chat.Id, "На сегодня ничего не запланировано, \nСэр !");
                    return;
                }

                _botMessage = await botClient.SendMessage(message.Chat.Id, "Проверяю список задач ...");

                foreach (var telagramMessage in tasks.Messages)
                {
                    _botMessage = await botClient.SendMessage(message.Chat.Id, telagramMessage,
                    parseMode: ParseMode.Markdown);

                    if (tasks.Messages.Count > 1) 
                        _botMessage = await botClient.SendMessage(message.Chat.Id, "смотрю еще ...");

                }
                _botMessage = await botClient.SendMessage(message.Chat.Id, "На сегодня все. \nСэр !");
            }
        }

        public async Task HandleGetTaskForWeekAsync(ITelegramBotClient botClient, Message message)
        {
            if (message.Text == "📅 Задачи недели")
            {
                //var emptyRequest = new TelegramEmptyRequest();
                //var tasks = await _grpcClient.TelegramGetTasksForTodayAsync(emptyRequest);

                //foreach (var telagramMessage in tasks.Messages)
                //{
                //    _botMessage = await botClient.SendMessage(message.Chat.Id, telagramMessage,
                //    parseMode: ParseMode.Markdown);
                //}

                _botMessage = await botClient.SendMessage(message.Chat.Id, "Функция еще не реализована.\nСэр !");
            }
        }

        public async Task HandleGetMenuFromBalukAsync(ITelegramBotClient botClient, Message message)
        {
            if (message.Text == "🍔 Меню - Балука")
            {
                //var emptyRequest = new TelegramEmptyRequest();
                //var tasks = await _grpcClient.TelegramGetTasksForTodayAsync(emptyRequest);

                //foreach (var telagramMessage in tasks.Messages)
                //{
                //    _botMessage = await botClient.SendMessage(message.Chat.Id, telagramMessage,
                //    parseMode: ParseMode.Markdown);
                //}

                _botMessage = await botClient.SendMessage(message.Chat.Id, "Функция под вопросом.\nСэр !");
            }
        }

        public async Task HandleChooseTasksButtonAsync(ITelegramBotClient botClient, Message message)
        {
            if (message.Text == "😎 Ассистент")
            {
                _botMessage = await botClient.SendMessage(message.Chat.Id, "Какие задачи нужны, \nСэр ?",
                        replyMarkup: _keyboardButtons.GetTasksMenuButtons());
            }
        }

        public async Task HandleUnknownMessageAsync(ITelegramBotClient botClient, Message message)
        {
            _botMessage = await botClient.SendMessage(message.Chat.Id, text: "Я отправлю эту информацию в архив, \nСэр !");
        }

        private async Task HandleMonitoringTasksAsync(ITelegramBotClient botClient, Message message,CancellationToken cancellationToken = default)
        {
            if (message.Text != "📋 Мониторинги")
            {
                return;
            }

            using var scope = _serviceProvider.CreateScope();

            var repository = scope.ServiceProvider.GetRequiredService<IWatchTaskRepository>();

            var tasks = await repository.GetAllAsync(cancellationToken);

            if (tasks.Count == 0)
            {
                await botClient.SendMessage(message.Chat.Id, "📋 У вас пока нет мониторингов.", cancellationToken: cancellationToken);

                return;
            }

            var builder = new StringBuilder();

            builder.AppendLine("📋 Ваши мониторинги:");
            builder.AppendLine();

            foreach (var task in tasks)
            {
                var status = task.IsEnabled ? "🟢" : "🔴";

                builder.AppendLine($"{status} {task.Name}");
                builder.AppendLine($"🌐 {task.Url}");
                builder.AppendLine($"🔎 {task.ConditionValue}");
                builder.AppendLine($"⏱ Интервал: {task.Interval}");
                builder.AppendLine();
            }

            await botClient.SendMessage(message.Chat.Id, builder.ToString(), 
                            cancellationToken: cancellationToken,
                            replyMarkup: _keyboardButtons.GetMonitoringMenuButtons());
        }

        private async Task<bool> HandleAddMonitoringAsync(ITelegramBotClient botClient, Message message)
        {
            if (message.Text != "➕ Добавить мониторинг")
            {
                return false;
            }

            _monitoringCreationStates[message.Chat.Id] =
                new MonitoringCreationState
                {
                    Step = MonitoringCreationStep.WaitingForName
                };

            await botClient.SendMessage(message.Chat.Id,"➕ Создание мониторинга\n\nВведите название:");

            return true;
        }

        private async Task HandleMonitoringCreationAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            if (!_monitoringCreationStates.TryGetValue(message.Chat.Id, out var state))
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(message.Text))
            {
                return;
            }

            if (message.Text.Equals("/cancel", StringComparison.OrdinalIgnoreCase))
            {
                _monitoringCreationStates.Remove(message.Chat.Id);

                await botClient.SendMessage(message.Chat.Id, "❌ Создание мониторинга отменено.");

                return;
            }

            switch (state.Step)
            {
                case MonitoringCreationStep.WaitingForName:
                    state.Name = message.Text.Trim();
                    state.Step = MonitoringCreationStep.WaitingForUrl;

                    await botClient.SendMessage(message.Chat.Id, "Введите URL страницы:");

                    break;

                case MonitoringCreationStep.WaitingForUrl:

                    if (!Uri.TryCreate(message.Text.Trim(), UriKind.Absolute, out var uri))
                    {
                        await botClient.SendMessage(message.Chat.Id,
                            "❌ Некорректный URL.\n\n" +
                            "Пример:\nhttps://example.com");

                        return;
                    }

                    state.Url = uri.ToString();
                    state.Step = MonitoringCreationStep.WaitingForCondition;

                    await botClient.SendMessage(message.Chat.Id,
                        "Что искать на странице?\n\n" +
                        "Например: Example Domain");

                    break;

                case MonitoringCreationStep.WaitingForCondition:

                    state.ConditionValue = message.Text.Trim();
                    state.Step = MonitoringCreationStep.WaitingForInterval;

                    await botClient.SendMessage(message.Chat.Id,
                        "Введите интервал проверки в секундах.\n\n" +
                        "Например: 60");

                    break;

                case MonitoringCreationStep.WaitingForInterval:

                    if (!int.TryParse(message.Text.Trim(), out var seconds) || seconds <= 0)
                    {
                        await botClient.SendMessage(message.Chat.Id, "❌ Введите положительное число секунд.\n\n" +
                            "Например: 60");

                        return;
                    }

                    state.Interval = TimeSpan.FromSeconds(seconds);
                    state.Step = MonitoringCreationStep.Confirming;

                    await botClient.SendMessage(message.Chat.Id,
                        $"Проверьте мониторинг:\n\n" +
                        $"📌 {state.Name}\n" +
                        $"🌐 {state.Url}\n" +
                        $"🔎 {state.ConditionValue}\n" +
                        $"⏱ {state.Interval}\n\n" +
                        "Напишите «да» для создания или «нет» для отмены.");

                    break;

                case MonitoringCreationStep.Confirming:

                    if (message.Text.Equals("нет", StringComparison.OrdinalIgnoreCase))
                    {
                        _monitoringCreationStates.Remove(message.Chat.Id);

                        await botClient.SendMessage( message.Chat.Id, "❌ Создание мониторинга отменено.");

                        return;
                    }

                    if (!message.Text.Equals("да", StringComparison.OrdinalIgnoreCase))
                    {
                        await botClient.SendMessage(message.Chat.Id, "Введите «да» или «нет».");

                        return;
                    }

                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var repository = scope.ServiceProvider.GetRequiredService<IWatchTaskRepository>();

                        var task = new WatchTask
                        {
                            Id = Guid.NewGuid(),
                            Name = state.Name!,
                            Url = new Uri(state.Url!),
                            ConditionType = ConditionType.TextExists,
                            ConditionValue = state.ConditionValue!,
                            Interval = state.Interval!.Value,
                            IsEnabled = true,
                            CreatedAt = DateTimeOffset.UtcNow
                        };

                        await repository.AddAsync(task, cancellationToken);
                    }

                    _monitoringCreationStates.Remove(message.Chat.Id);

                    await botClient.SendMessage(
                        message.Chat.Id,
                        $"✅ Мониторинг создан!\n\n" +
                        $"📌 {state.Name}\n" +
                        $"🌐 {state.Url}\n" +
                        $"🔎 {state.ConditionValue}\n" +
                        $"⏱ {state.Interval}\n\n" +
                        "Мониторинг будет автоматически запущен.");

                    break;
            }
        }

        private async Task HandleStopMonitoringAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            if (message.Text != "⏯️ Остановить мониторинг")
            {
                return;
            }

            await botClient.SendMessage(
    message.Chat.Id,
    "ТЕСТ: дошли до HandleStopMonitoringAsync");

            using var scope = _serviceProvider.CreateScope();

            var repository = scope.ServiceProvider.GetRequiredService<IWatchTaskRepository>();

            var tasks = await repository.GetAllAsync(cancellationToken);

            var enabledTasks = tasks
                .Where(x => x.IsEnabled)
                .ToList();

            if (enabledTasks.Count == 0)
            {
                await botClient.SendMessage(message.Chat.Id, "⏯️ Нет запущенных мониторингов.", cancellationToken: cancellationToken);
                return;
            }

            var keyboard = _keyboardButtons.GetStopMonitoringButtons(enabledTasks);

            await botClient.SendMessage(message.Chat.Id, "⏯️ Выберите мониторинг для остановки:",
                    replyMarkup: keyboard, cancellationToken: cancellationToken);
        }

        private async Task HandleMonitorStopCallbackQueryAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            if (callbackQuery.Data == null)
            {
                return;
            }

            if (callbackQuery.Data.StartsWith("monitor_stop:"))
            {
                await StopMonitoringAsync(
                    botClient,
                    callbackQuery,
                    cancellationToken);

                return;
            }
        }

        private async Task HandleStartMonitoringAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            if (message.Text != "⏩ Запустить мониторинг")
            {
                return;
            }

            using var scope = _serviceProvider.CreateScope();

            var repository = scope.ServiceProvider.GetRequiredService<IWatchTaskRepository>();

            var tasks = await repository.GetAllAsync(cancellationToken);

            var disabledTasks = tasks
                .Where(x => !x.IsEnabled)
                .ToList();

            if (disabledTasks.Count == 0)
            {
                await botClient.SendMessage(message.Chat.Id, "⏩ Нет остановленных мониторингов.", cancellationToken: cancellationToken);
                return;
            }

            var keyboard = _keyboardButtons.GetStartMonitoringButtons(disabledTasks);

            await botClient.SendMessage(message.Chat.Id, "⏩ Выберите мониторинг для запуска:",
                replyMarkup: keyboard, cancellationToken: cancellationToken);
        }

        private async Task HandleMonitorStartCallbackQueryAsync(ITelegramBotClient botClient, CallbackQuery query)
        {
            if (query.Data is null || !query.Data.StartsWith("monitor_start:"))
            {
                return;
            }

            var idString = query.Data["monitor_start:".Length..];

            if (!Guid.TryParse(idString, out var taskId))
            {
                await botClient.AnswerCallbackQuery(query.Id, "❌ Некорректный идентификатор мониторинга.");
                return;
            }

            using var scope = _serviceProvider.CreateScope();

            var repository = scope.ServiceProvider.GetRequiredService<IWatchTaskRepository>();

            var task = await repository.GetByIdAsync(taskId);

            if (task is null)
            {
                await botClient.AnswerCallbackQuery(query.Id, "❌ Мониторинг не найден.");
                return;
            }

            if (task.IsEnabled)
            {
                await botClient.AnswerCallbackQuery(query.Id, "Мониторинг уже запущен.");
                return;
            }

            task.IsEnabled = true;

            await repository.UpdateAsync(task);

            await botClient.AnswerCallbackQuery(query.Id, "▶️ Мониторинг запущен.");

            await botClient.SendMessage(query.Message!.Chat.Id, $"▶️ Мониторинг запущен:\n\n📌 {task.Name}");
        }

        private async Task StopMonitoringAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            var taskIdString = callbackQuery.Data!.Replace("monitor_stop:", "");

            if (!Guid.TryParse(taskIdString, out var taskId))
            {
                await botClient.AnswerCallbackQuery(callbackQuery.Id,
                    "❌ Некорректный идентификатор задачи.",
                    cancellationToken: cancellationToken);

                return;
            }

            using var scope = _serviceProvider.CreateScope();

            var repository = scope.ServiceProvider.GetRequiredService<IWatchTaskRepository>();

            var task = await repository.GetByIdAsync(taskId, cancellationToken);

            if (task == null)
            {
                await botClient.AnswerCallbackQuery(
                    callbackQuery.Id,
                    "❌ Мониторинг не найден.",
                    cancellationToken: cancellationToken);

                return;
            }

            if (!task.IsEnabled)
            {
                await botClient.AnswerCallbackQuery(
                    callbackQuery.Id,
                    "Мониторинг уже остановлен.",
                    cancellationToken: cancellationToken);

                return;
            }

            task.IsEnabled = false;

            await repository.UpdateAsync(task, cancellationToken);

            await botClient.AnswerCallbackQuery(callbackQuery.Id, "⏯️ Мониторинг остановлен.", cancellationToken: cancellationToken);

            await botClient.SendMessage(
                callbackQuery.Message!.Chat.Id,
                $"⏹ Мониторинг остановлен:\n\n" +
                $"📌 {task.Name}",
                cancellationToken: cancellationToken);
        }

        private async Task HandleDeleteMonitoringAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            if (message.Text != "➖ Удалить мониторинг")
            {
                return;
            }

            using var scope = _serviceProvider.CreateScope();

            var repository =
                scope.ServiceProvider.GetRequiredService<IWatchTaskRepository>();

            var tasks = await repository.GetAllAsync(cancellationToken);

            if (tasks.Count == 0)
            {
                await botClient.SendMessage(message.Chat.Id, "➖ Мониторингов нет.", cancellationToken: cancellationToken);

                return;
            }

            var keyboard =_keyboardButtons.GetDeleteMonitoringButtons(tasks);

            await botClient.SendMessage(message.Chat.Id, "➖ Выберите мониторинг для удаления:",
                replyMarkup: keyboard, cancellationToken: cancellationToken);
        }

        private async Task HandleMonitorDeleteCallbackQueryAsync(ITelegramBotClient botClient, CallbackQuery query)
        {
            if (query.Data is null || !query.Data.StartsWith("monitor_delete:"))
            {
                return;
            }

            if (!Guid.TryParse(query.Data["monitor_delete:".Length..], out var taskId))
            {
                return;
            }

            using var scope = _serviceProvider.CreateScope();

            var repository =
                scope.ServiceProvider.GetRequiredService<IWatchTaskRepository>();

            var task = await repository.GetByIdAsync(taskId);

            if (task is null)
            {
                await botClient.SendMessage(query.Message!.Chat.Id, "❌ Мониторинг не найден.");

                return;
            }

            var monitoringService = scope.ServiceProvider.GetRequiredService<MonitoringService>();

            monitoringService.StopTask(task.Id);

            await repository.DeleteAsync(task.Id);

            await botClient.SendMessage(query.Message!.Chat.Id, $"➖ Мониторинг «{task.Name}» удалён.");
        }
    }
}
