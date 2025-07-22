using JarvisBot.Background;
using JarvisBot.Exchange.AlfaBankInSyncRates;
using JarvisBot.KeyboardButtons;
using JarvisBot.Weather;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Telegram.Bot;
using Telegram.Bot.Types;

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
        private bool _messageInProcess;


        public CommunicationMethods(JarvisClientSettings clientSettings, ExchangeRateLoder exchangeRateLoder, WeatherLoder weatherLoder)
        {
            _clientSettings = clientSettings;
            _exchangeRateLoder = exchangeRateLoder;
            _weatherLoder = weatherLoder;

            _adminChatId = new(_clientSettings.AdminChatId);
        }


        public async Task ProcessingMessage(ITelegramBotClient botClient, Message message, User botUsername, CancellationToken cancellationToken)
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

                if (_botMessage.Text == null || _botMessage.Text == string.Empty)
                {
                    await HandleUnknownMessageAsync(botClient, message);
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

        public async Task ProcessingCallback(ITelegramBotClient botClient, CallbackQuery query, User botUsername)
        {
            await HandleStartAnyDeskAsync(botClient, query);
            await HandleRebootPCAsync(botClient, query);

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
                _botMessage = await botClient.SendTextMessageAsync(message.Chat.Id, "Privet");
            }
        }

        public async Task HandleBackToMenuAsync(ITelegramBotClient botClient, Message message)
        {
            if (message.Text == "⬅️ Back")
            {
                if (message.Text == "⬅️ Back" && message.Chat.Id == _adminChatId)
                {
                    _botMessage = await botClient.SendTextMessageAsync(message.Chat.Id, "Вы в МЕНЮ",
                        replyMarkup: _keyboardButtons.GetAdminMenuButtons());
                }
                else
                {
                    _botMessage = await botClient.SendTextMessageAsync(message.Chat.Id, "Вы в МЕНЮ",
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
                    _botMessage = await botClient.SendTextMessageAsync(message.Chat.Id, text: "Choose",
                        replyMarkup: _keyboardButtons.GetAdminMenuButtons());
                }
                else
                {
                    _botMessage = await botClient.SendTextMessageAsync(message.Chat.Id, text: "Choose",
                        replyMarkup: _keyboardButtons.GetMenuButtons());
                }
            }
        }

        public async Task HandleCurrencyAsync(ITelegramBotClient botClient, Message message)
        {
            if (message.Text.Contains("Курсы валют", StringComparison.CurrentCultureIgnoreCase))
            {
                _botMessage = await botClient.SendTextMessageAsync(message.Chat.Id, text: "Выберите валюту",
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
                    var rateMessage = _exchangeRateLoder.RatesResponse(message.Text, _cancellationToken.Token);
                    _botMessage = await botClient.SendTextMessageAsync(message.Chat.Id, rateMessage);
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
                    _botMessage = await botClient.SendTextMessageAsync(message.Chat.Id, text: "Авто обновление курса валют ...",
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
                    _botMessage = await botClient.SendTextMessageAsync(message.Chat.Id, text: "Обновление Курса валют ВКЛЮЧЕНО",
                        replyMarkup: _keyboardButtons.GetAdminMenuButtons());

                    SetTimer(_cancellationToken.Token);
                }
                else if (message.Text == "Stop 🔄️")
                {
                    _botMessage = await botClient.SendTextMessageAsync(message.Chat.Id, text: "Авто обновление ВЫКЛЮЧЕНО",
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
                        updateRate = _exchangeRateLoder.EqualityCurrencyExchangeRate(rate, cancellationToken);

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
            _botMessage = await botClient.SendTextMessageAsync(message.Chat.Id, rateMessage);
        }

        public async Task HandleWeatherAsync(ITelegramBotClient botClient, Message message)
        {
            if (message.Text == "☂️ Погода")
            {
                var weatuerMessage = _weatherLoder.WeatherResponse();
                _botMessage = await botClient.SendTextMessageAsync(message.Chat.Id, await weatuerMessage);
            }
        }

        public async Task HandleHelpButtonAsync(ITelegramBotClient botClient, Message message)
        {
            if (message.Text.Contains("🙋‍♂️ Help", StringComparison.CurrentCultureIgnoreCase) && message.Chat.Id == _adminChatId)
            {
                _botMessage = await botClient.SendTextMessageAsync(message.Chat.Id, text: "Что-то включить?",
                    replyMarkup: _keyboardButtons.GetHelpSubmenuButtons());
            }
        }

        public async Task HandleDeviceButtonAsync(ITelegramBotClient botClient, Message message)
        {
            if (message.Text == "💻 Device" && message.Chat.Id == _adminChatId)
            {
                _botMessage = await botClient.SendTextMessageAsync(message.Chat.Id, text: "Вы в меню управления программой - [AnyDesk]",
                    replyMarkup: _keyboardButtons.GetStartAnyDeskButtons());

                _botMessage = await botClient.SendTextMessageAsync(message.Chat.Id, text: "Что сделать с программой, Сэр?",
                    replyMarkup: _keyboardButtons.GetBackButtons());
            }
        }

        public async Task HandleStartAnyDeskAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery)
        {
            if (callbackQuery.Data == "Start_AnyDesk")
            {
                _botMessage = await botClient.SendTextMessageAsync(_botMessage.Chat.Id, "AnyDesk включается...");
                StartAnyDesk(botClient, _botMessage);
            }
            else if (callbackQuery.Data == "Cancel_AnyDesk")
            {
                _botMessage = await botClient.SendTextMessageAsync(_botMessage.Chat.Id, "Выключение AnyDesk.", replyMarkup: _keyboardButtons.GetAdminMenuButtons());
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
                        _botMessage = await botClient.SendTextMessageAsync(message.Chat.Id, "AnyDesk запущен");
                        Console.WriteLine("AnyDesk запущен");
                    }
                    else
                    {
                        _botMessage = await botClient.SendTextMessageAsync(message.Chat.Id, "Проблемы с запуском...");
                    }
                }
                catch (System.ComponentModel.Win32Exception ex)
                {
                    Console.WriteLine($"Ошибка: {ex.Message}");
                }
            }
            else
            {
                botClient.SendTextMessageAsync(message.Chat.Id, "AnyDesk уже запущен");
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
                _botMessage = await botClient.SendTextMessageAsync(message.Chat.Id, "AnyDesk закрыт");
                Console.WriteLine("AnyDesk закрыт");
            }
            else
            {
                _botMessage = await botClient.SendTextMessageAsync(message.Chat.Id, "... закрываем AnyDesk повторно");
                Console.WriteLine("... закрываем AnyDesk повторно");

                await CloseAnyDeskProcesses(botClient, message);

                _botMessage = await botClient.SendTextMessageAsync(message.Chat.Id, "AnyDesk закрыт");
                Console.WriteLine("AnyDesk закрыт");
            }

            return true;
        }

        private async Task CloseAnyDeskProcesses(ITelegramBotClient botClient, Message message)
        {
            foreach (var process in Process.GetProcessesByName("AnyDesk"))
            {
                process.Kill();

                _botMessage = await botClient.SendTextMessageAsync(message.Chat.Id, "AnyDesk закрывается...");
                Console.WriteLine("AnyDesk закрывается...");
            }
        }

        public async Task HandleRebootButtonAsync(ITelegramBotClient botClient, Message message)
        {
            if (message.Text == "🛠️ Something" && message.Chat.Id == _adminChatId)
            {
                _botMessage = await botClient.SendTextMessageAsync(message.Chat.Id, text: "ВНИМАНИЕ !!! \r\nВы вошли в настройки управления компьютером:",
                    replyMarkup: _keyboardButtons.GetRebootButtons());

                _botMessage = await botClient.SendTextMessageAsync(message.Chat.Id, text: "Еще не поздно вернуться назад, Сэр.",
                    replyMarkup: _keyboardButtons.GetBackButtons());
            }
        }

        public async Task HandleRebootPCAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery)
        {
            var chatId = _botMessage.Chat?.Id ?? 552523783;

            if (callbackQuery.Data == "PC_Reboot")
            {
                _botMessage = await botClient.SendTextMessageAsync(chatId, "Ждите компьютер ПЕРЕЗАГРУЖАЕТСЯ...");
                RebootPcClick(botClient, _botMessage);
            }
            else if (callbackQuery.Data == "PC_PowerOFF")
            {
                _botMessage = await botClient.SendTextMessageAsync(chatId, "ВЫКЛЮЧЕНИЕ компьютера...");
                PowerOffPcClick(botClient, _botMessage);
            }
            else if (callbackQuery.Data == "PC_Lock")
            {
                _botMessage = await botClient.SendTextMessageAsync(chatId, "Выключение экрана ...");
                LockPcClick(botClient, _botMessage);
            }
        }

        public async void RebootPcClick(ITelegramBotClient botClient, Message message)
        {
            _botMessage = await botClient.SendTextMessageAsync(message.Chat.Id, "Ждите Я скоро ..!");
            Console.WriteLine("Ждите Я скоро ..!");

            string rebootPC = "shutdown";
            string arguments = "/r /t 1";
            Process.Start(rebootPC, arguments);
        }

        public async void PowerOffPcClick(ITelegramBotClient botClient, Message message)
        {
            _botMessage = await botClient.SendTextMessageAsync(message.Chat.Id, "До скорого, сэр");
            Console.WriteLine("До скорого, сэр");

            string powerOffPC = "shutdown";
            string arguments = "/s /f /t 0";
            Process.Start(powerOffPC, arguments);
        }

        public async void LockPcClick(ITelegramBotClient botClient, Message message)
        {
            _botMessage = await botClient.SendTextMessageAsync(message.Chat.Id, "Экран выключен, сер");
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

        public async Task HandleUnknownMessageAsync(ITelegramBotClient botClient, Message message)
        {
            _botMessage = await botClient.SendTextMessageAsync(message.Chat.Id, text: "Я отправлю эту информацию в архив, Сэр !");
        }
    }
}
